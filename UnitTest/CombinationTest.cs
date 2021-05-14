using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using GPC.Utilities.Extensions;
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
        private void CommonAssert(Combination combo1, Combination combo2)
        {
            Assert.IsTrue(combo1.GetLoadCases().ScrambledEquals(combo2.GetLoadCases()), $"Combination {combo1.Name} e {combo2.Name} are not equals");

            List<double> comboCoef1 = combo1.GetLoadCaseCoefficients(out List<LoadCaseBase> list1);
            List<double> comboCoef2 = combo2.GetLoadCaseCoefficients(out List<LoadCaseBase> list2);

            Assert.IsTrue(comboCoef2.Count() == comboCoef1.Count());
            Assert.IsTrue(combo1.LoadCaseCount == combo2.LoadCaseCount);
            Assert.IsTrue(list1.Count() == list2.Count());
            Assert.IsTrue(list1.ScrambledEquals(list2));

            for (int i = 0; i < comboCoef1.Count(); i++)
            {
                Assert.IsTrue(Math.Abs(comboCoef1[i] - comboCoef2[i]) < 0.001, $"Coefficient Combo {i} error! Load Case. {list1[i].Name} comboCoef 1: {comboCoef1[i]} ; comboCoef 2: {comboCoef2[i]}");
            }
        }


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

            loadCases.Add(new LoadCase("Snow", LoadCase.LoadCaseTypes.Snow));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseTypes.LiveLoad));
            coefficients.Add(1);

            LoadCase lcSw = new LoadCase("SW", LoadCase.LoadCaseTypes.SelfWeight);
            loadCases.Add(lcSw);
            coefficients.Add(0.5);

            LoadCase lcSdl = new LoadCase("SDL", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            loadCases.Add(lcSdl);
            coefficients.Add(4);

            Combination combination = new Combination("test");

            combination.AddLoadCaseCoefficients(loadCases, coefficients);

            // Act
            string combinationName = combination.ToString();

            // Assert
            var splitted = combinationName.Split(new string[] { "+" }, StringSplitOptions.None);

            Console.WriteLine(combinationName);

            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == lcSw).Count() == 1);
            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == lcSw).SingleOrDefault().coefficient == 0.5);
            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == lcSdl).Count() == 1);
            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == lcSdl).SingleOrDefault().coefficient == 4);
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

            LoadCase lcSdl = new LoadCase("SW", LoadCase.LoadCaseTypes.SelfWeight, Guid.NewGuid());
            loadCases.Add(lcSdl);
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
            Console.WriteLine(combinationName);

            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == sdl).Count() == 1);
            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == sdl).SingleOrDefault().coefficient == 8);
            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == lcSdl).Count() == 1);
            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == lcSdl).SingleOrDefault().coefficient == 0.5);
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
            coefficients.Add(2);

            Combination combination = new Combination("test");

            combination.AddLoadCaseCoefficients(loadCases, coefficients);

            // Act
            string combinationName = combination.ToString();

            // Assert
            Console.WriteLine(combinationName);

            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == sdl).Count() == 1);
            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == sdl).SingleOrDefault().coefficient == 6);
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
            Console.WriteLine(combinationName);
            Assert.IsTrue(combination[sdl] == 8, combinationName);
            Assert.IsTrue(combination[new LoadCaseBase("test")] == 0, combinationName);
            Assert.IsTrue(combination[new LoadCase("Zero", LoadCase.LoadCaseTypes.Earthquake)] == 0, combinationName);
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


            Assert.IsTrue(pair[2].Key == lc1, pair[0].Key.Name.ToString());
            Assert.IsTrue(pair[3].Key == lc2, pair[1].Key.Name.ToString());
            Assert.IsTrue(tuple[2].loadcase == lc1, tuple[0].loadcase.Name.ToString());
            Assert.IsTrue(tuple[3].loadcase == lc2, tuple[1].loadcase.Name.ToString());

            Assert.IsTrue(tuple2.Length == 2, tuple2.Length.ToString());
            Assert.IsTrue(tuple2[0].coefficient == 1, tuple2[0].coefficient.ToString());

            Assert.IsTrue(tuple3.Length == 1, tuple3.Length.ToString());
            Assert.IsTrue(tuple3[0].coefficient == 2, tuple3[0].coefficient.ToString());
        }

        #endregion

        #region GENERATE COMBINATIONS EN1990

        [TestMethod]
        public void EN1990GeneratorUltimateStructural1()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryC, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

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
            Assert.IsTrue(listComb.Count() == 3);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateStructural2()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.5);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.5);
            
            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination3.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase, 1.5);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 1.05);
            
            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination4.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase, 1.5);

            // Assert
            Assert.IsTrue(listComb.Count() == 6);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateStructural3()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryE, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            // categoria E ha gli psi tutti a 1.0
            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(liveLoadLoadCase, 1.30);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.30);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.65);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(liveLoadLoadCase, 1.30);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.78);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.30);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(liveLoadLoadCase, 1.30);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase, 0.78);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 0.65);

            // Assert
            Assert.IsTrue(listComb.Count() == 4);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateEquilibrium1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "SuperImposedDeadLoad";
            LoadCase superImposedDeadLoadLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.SuperImposedDeadLoad);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                superImposedDeadLoadLoadCase,
                snowLoadCase,
                selfWeightLoadCase,
                WindPressureLoadCase,
            };

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateEquilibrium, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryD, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 0.90);
            combination1.AddLoadCaseCoefficient(superImposedDeadLoadLoadCase, 0.90);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(superImposedDeadLoadLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 0.75);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.10);
            combination3.AddLoadCaseCoefficient(superImposedDeadLoadLoadCase, 1.10);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.10);
            combination4.AddLoadCaseCoefficient(superImposedDeadLoadLoadCase, 1.10);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination4.AddLoadCaseCoefficient(snowLoadCase, 0.75);

            // Assert
            Assert.IsTrue(listComb.Count() == 6);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateEquilibrium2()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateEquilibrium, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryA, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 0.90);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.75);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.10);
            combination3.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 0.75);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.10);
            combination4.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            // Assert
            Assert.IsTrue(listComb.Count() == 6);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateEquilibrium3()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateEquilibrium, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryD, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

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
            Assert.IsTrue(listComb.Count() == 6);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateFatigue1()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateFatigue, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryG, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

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
            Assert.IsTrue(listComb.Count() == 3);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateFatigue2()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateFatigue, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

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
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination3.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination4 = new Combination("cmb 2", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination4.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            // Assert
            Assert.IsTrue(listComb.Count() == 6);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateFatigue3()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateFatigue, ImposedLoadCategories.CategoryG, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

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
            Assert.IsTrue(listComb.Count() == 6);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateGeotechnical1()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateGeotechnical, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryF, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

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
            Assert.IsTrue(listComb.Count() == 6);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateGeotechnical2()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateGeotechnical, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

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
            Assert.IsTrue(listComb.Count() == 6);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateGeotechnical3()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateGeotechnical, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryF, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

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
            Assert.IsTrue(listComb.Count() == 3);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
        }

        [TestMethod]
        public void EN1990GeneratorServiceabilityCharacteristic()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.ServiceabilityCharacteristic, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

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
            Assert.IsTrue(listComb.Count() == 3);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
        }

        [TestMethod]
        public void EN1990GeneratorServiceabilityQuasiPermanent()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.ServiceabilityQuasiPermanent, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.20);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);

            // Assert
            Assert.IsTrue(listComb.Count() == 2);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
        }

        [TestMethod]
        public void EN1990GeneratorServiceabilityFrequent()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.ServiceabilityFrequent, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.2);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 0.2);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 0.50);

            // Assert
            Assert.IsTrue(listComb.Count() == 3);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
        }

        [TestMethod]
        public void EN1990GeneratorMultyLoadCase()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase2, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase2, 1.50);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase3, 1.50);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase4, 1.50);
            combination1.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination1.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase2, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase3, 0.90);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase4, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination2.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase2, 1.35);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase2, 1.50);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase3, 1.50);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase4, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination3.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase2, 1.35);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase3, 0.90);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase4, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination4.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            // Assert
            Assert.IsTrue(listComb.Count() == 6);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);

        }

        [TestMethod]
        public void EN1990GeneratorMultyLoadCase2()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase2, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase2, 1.50);
            combination1.AddLoadCaseCoefficient(temperatureLoadCase3, 0.90);
            combination1.AddLoadCaseCoefficient(temperatureLoadCase4, 0.90);
            combination1.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination1.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase2, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination2.AddLoadCaseCoefficient(temperatureLoadCase3, 1.50);
            combination2.AddLoadCaseCoefficient(temperatureLoadCase4, 1.50);
            combination2.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination2.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase2, 1.00);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination3.AddLoadCaseCoefficient(temperatureLoadCase3, 0.90);
            combination3.AddLoadCaseCoefficient(temperatureLoadCase4, 0.90);
            combination3.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase2, 1.35);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase1, 1.50);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase2, 1.50);
            combination4.AddLoadCaseCoefficient(temperatureLoadCase3, 0.90);
            combination4.AddLoadCaseCoefficient(temperatureLoadCase4, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination4.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination5 = new Combination("cmb 5", options);
            combination5.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination5.AddLoadCaseCoefficient(selfWeightLoadCase2, 1.35);
            combination5.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination5.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination5.AddLoadCaseCoefficient(temperatureLoadCase3, 1.50);
            combination5.AddLoadCaseCoefficient(temperatureLoadCase4, 1.50);
            combination5.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination5.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination6 = new Combination("cmb 6", options);
            combination6.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination6.AddLoadCaseCoefficient(selfWeightLoadCase2, 1.35);
            combination6.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination6.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination6.AddLoadCaseCoefficient(temperatureLoadCase3, 0.90);
            combination6.AddLoadCaseCoefficient(temperatureLoadCase4, 0.90);
            combination6.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination6.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            // Assert
            Assert.IsTrue(listComb.Count() == 8);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
            CommonAssert(listComb[4], combination5);
            CommonAssert(listComb[5], combination6);
        }

        [TestMethod]
        public void EN1990GeneratorWindPressureSuction1()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(windPressureLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(windPressureLoadCase2, 1.50);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(windSuctionLoadCase1, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination3.AddLoadCaseCoefficient(windPressureLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(windPressureLoadCase2, 1.50);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination4.AddLoadCaseCoefficient(windSuctionLoadCase1, 1.50);

            // Assert
            Assert.IsTrue(listComb.Count() == 6);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
        }

        [TestMethod]
        public void EN1990GeneratorWindPressureSuction2()
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
            string loadCaseName6 = "Temperature1";
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

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
            combination7.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination7.AddLoadCaseCoefficient(windPressureLoadCase1, 1.50);
            combination7.AddLoadCaseCoefficient(windPressureLoadCase2, 1.50);
            combination7.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination7.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination7.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination8 = new Combination("cmb 8", options);
            combination8.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination8.AddLoadCaseCoefficient(windSuctionLoadCase1, 1.50);
            combination8.AddLoadCaseCoefficient(windSuctionLoadCase2, 1.50);
            combination8.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination8.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination8.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination9 = new Combination("cmb 9", options);
            combination9.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination9.AddLoadCaseCoefficient(windPressureLoadCase1, 0.90);
            combination9.AddLoadCaseCoefficient(windPressureLoadCase2, 0.90);
            combination9.AddLoadCaseCoefficient(temperatureLoadCase1, 1.50);
            combination9.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination9.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination10 = new Combination("cmb 10", options);
            combination10.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination10.AddLoadCaseCoefficient(windSuctionLoadCase1, 0.90);
            combination10.AddLoadCaseCoefficient(windSuctionLoadCase2, 0.90);
            combination10.AddLoadCaseCoefficient(temperatureLoadCase1, 1.50);
            combination10.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination10.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination11 = new Combination("cmb 11", options);
            combination11.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination11.AddLoadCaseCoefficient(windPressureLoadCase1, 0.90);
            combination11.AddLoadCaseCoefficient(windPressureLoadCase2, 0.90);
            combination11.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination11.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination11.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            Combination combination12 = new Combination("cmb 12", options);
            combination12.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination12.AddLoadCaseCoefficient(windSuctionLoadCase1, 0.90);
            combination12.AddLoadCaseCoefficient(windSuctionLoadCase2, 0.90);
            combination12.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination12.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination12.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            // Assert
            Assert.IsTrue(listComb.Count() == 14);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
            CommonAssert(listComb[4], combination5);
            CommonAssert(listComb[5], combination6);
            CommonAssert(listComb[6], combination7);
            CommonAssert(listComb[7], combination8);
            CommonAssert(listComb[8], combination9);
            CommonAssert(listComb[9], combination10);
            CommonAssert(listComb[10], combination11);
            CommonAssert(listComb[11], combination12);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateSeismic1()
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

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateSeismic, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryC, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(seismicLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(liveLoadLoadCase, 0.60);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(seismicLoadCase, 1.00);

            // Assert
            Assert.IsTrue(listComb.Count() == 2);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateSeismic2()
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
            LoadCase earthquakeLoadCase = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.Earthquake);
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
                earthquakeLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
                liveLoadLoadCase3,
            };

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateSeismic, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryC, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(earthquakeLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 0.00);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.00);
            combination1.AddLoadCaseCoefficient(liveLoadLoadCase1, 0.60);
            combination1.AddLoadCaseCoefficient(liveLoadLoadCase2, 0.60);
            combination1.AddLoadCaseCoefficient(liveLoadLoadCase3, 0.60);


            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(earthquakeLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);

            // Assert
            Assert.IsTrue(listComb.Count() == 2);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
        }

        [TestMethod]
        public void EN1990GeneratorUltimateSeismic3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName5 = "Earthquake";
            LoadCase earthquakeLoadCase = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.Earthquake);
            string loadCaseName7 = "LiveLoad1";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad2";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                earthquakeLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
            };

            EN1990CombinationsOptions options = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateSeismic, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryC, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(earthquakeLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 0.00);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.20);
            combination1.AddLoadCaseCoefficient(liveLoadLoadCase1, 0.60);
            combination1.AddLoadCaseCoefficient(liveLoadLoadCase2, 0.60);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(earthquakeLoadCase, 1.00);

            // Assert
            Assert.IsTrue(listComb.Count() == 2);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
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

            StandardEN16612.EN16612CombinationsOptions options = new StandardEN16612.EN16612CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB,
                ImposedLoadCategories.CategoryA, true);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaHLoadCase1, 1.00);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination3.AddLoadCaseCoefficient(climateSummerDeltaHLoadCase1, 1.35);
            combination3.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination4.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination4.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 10);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
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

            StandardEN16612.EN16612CombinationsOptions options = new StandardEN16612.EN16612CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB,
                ImposedLoadCategories.CategoryA, true);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaHLoadCase1, 1.00);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination3.AddLoadCaseCoefficient(climateSummerDeltaHLoadCase1, 1.35);
            combination3.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination3.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination4.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination4.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination4.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination4.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 10);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
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

            StandardEN16612.EN16612CombinationsOptions options = new StandardEN16612.EN16612CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaHLoadCase1, 1.00);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase2, 1.50);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination4.AddLoadCaseCoefficient(climateSummerDeltaHLoadCase1, 1.35);
            combination4.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination4.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination4.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination4.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);

            Combination combination5 = new Combination("cmb 5", options);
            combination5.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination5.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination5.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination5.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination5.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);

            Combination combination6 = new Combination("cmb 6", options);
            combination6.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination6.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase1, 1.50);
            combination6.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase2, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 12);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
            CommonAssert(listComb[4], combination5);
            CommonAssert(listComb[5], combination6);
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

            StandardEN16612.EN16612CombinationsOptions options = new StandardEN16612.EN16612CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB,
                ImposedLoadCategories.CategoryA, true);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaHLoadCase1, 1.00);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination1.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);
            combination1.AddLoadCaseCoefficient(windLoadCase1, 0.90);
            combination1.AddLoadCaseCoefficient(windLoadCase2, 0.90);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination2.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);
            combination2.AddLoadCaseCoefficient(windLoadCase1, 0.90);
            combination2.AddLoadCaseCoefficient(windLoadCase2, 0.90);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase2, 1.50);
            combination3.AddLoadCaseCoefficient(windLoadCase1, 0.90);
            combination3.AddLoadCaseCoefficient(windLoadCase2, 0.90);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(windLoadCase1, 1.50);
            combination4.AddLoadCaseCoefficient(windLoadCase2, 1.50);
            combination4.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase1, 0.45);
            combination4.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase2, 0.45);

            Combination combination5 = new Combination("cmb 5", options);
            combination5.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination5.AddLoadCaseCoefficient(climateSummerDeltaHLoadCase1, 1.00);
            combination5.AddLoadCaseCoefficient(windLoadCase1, 1.50);
            combination5.AddLoadCaseCoefficient(windLoadCase2, 1.50);
            combination5.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 0.45);
            combination5.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 0.45);
            combination5.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 0.45);
            combination5.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 0.45);

            Combination combination6 = new Combination("cmb 6", options);
            combination6.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination6.AddLoadCaseCoefficient(windLoadCase1, 1.50);
            combination6.AddLoadCaseCoefficient(windLoadCase2, 1.50);
            combination6.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 0.45);
            combination6.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 0.45);
            combination6.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 0.45);
            combination6.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 0.45);

            Combination combination7 = new Combination("cmb 7", options);
            combination7.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination7.AddLoadCaseCoefficient(climateSummerDeltaHLoadCase1, 1.35);
            combination7.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination7.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination7.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination7.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);
            combination7.AddLoadCaseCoefficient(windLoadCase1, 0.90);
            combination7.AddLoadCaseCoefficient(windLoadCase2, 0.90);

            Combination combination8 = new Combination("cmb 8", options);
            combination8.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination8.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 1.50);
            combination8.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 1.50);
            combination8.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 1.50);
            combination8.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 1.50);
            combination8.AddLoadCaseCoefficient(windLoadCase1, 0.90);
            combination8.AddLoadCaseCoefficient(windLoadCase2, 0.90);

            Combination combination9 = new Combination("cmb 9", options);
            combination9.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination9.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase1, 1.50);
            combination9.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase2, 1.50);
            combination9.AddLoadCaseCoefficient(windLoadCase1, 0.90);
            combination9.AddLoadCaseCoefficient(windLoadCase2, 0.90);

            Combination combination10 = new Combination("cmb 10", options);
            combination10.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination10.AddLoadCaseCoefficient(windLoadCase1, 1.50);
            combination10.AddLoadCaseCoefficient(windLoadCase2, 1.50);
            combination10.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase1, 0.45);
            combination10.AddLoadCaseCoefficient(climateWinterDeltaPLoadCase2, 0.45);

            Combination combination11 = new Combination("cmb 11", options);
            combination11.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination11.AddLoadCaseCoefficient(climateSummerDeltaHLoadCase1, 1.35);
            combination11.AddLoadCaseCoefficient(windLoadCase1, 1.50);
            combination11.AddLoadCaseCoefficient(windLoadCase2, 1.50);
            combination11.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 0.45);
            combination11.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 0.45);
            combination11.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 0.45);
            combination11.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 0.45);

            Combination combination12 = new Combination("cmb 12", options);
            combination12.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination12.AddLoadCaseCoefficient(windLoadCase1, 1.50);
            combination12.AddLoadCaseCoefficient(windLoadCase2, 1.50);
            combination12.AddLoadCaseCoefficient(climateSummerDeltaTLoadCase1, 0.45);
            combination12.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase1, 0.45);
            combination12.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase2, 0.45);
            combination12.AddLoadCaseCoefficient(climateSummerDeltaPLoadCase3, 0.45);

            // Assert
            Assert.IsTrue(outList.Count() == 20);
            CommonAssert(listComb[0], combination1);
            CommonAssert(listComb[1], combination2);
            CommonAssert(listComb[2], combination3);
            CommonAssert(listComb[3], combination4);
            CommonAssert(listComb[4], combination5);
            CommonAssert(listComb[5], combination6);
            CommonAssert(listComb[6], combination7);
            CommonAssert(listComb[7], combination8);
            CommonAssert(listComb[8], combination9);
            CommonAssert(listComb[9], combination10);
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

            StandardEN16612.EN16612CombinationsOptions options = new StandardEN16612.EN16612CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryC, false);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }
        
            // Assert
            Assert.IsTrue(outList.Count() == 30);
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

            StandardEN16612.EN16612CombinationsOptions options = new StandardEN16612.EN16612CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryC, false);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            // Assert
            Assert.IsTrue(outList.Count() == 30);
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

            StandardEN16612.EN16612CombinationsOptions options = new StandardEN16612.EN16612CombinationsOptions(StandardEN1990.LimitStates.ServiceabilityCharacteristic, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryC, false);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            // Assert
            Assert.IsTrue(outList.Count() == 10);
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

            StandardEN16612.EN16612CombinationsOptions options = new StandardEN16612.EN16612CombinationsOptions(StandardEN1990.LimitStates.ServiceabilityCharacteristic, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryC, false);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            List<Combination> listComb = new List<Combination>();
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
                listComb.Add(combination);
            }

            // Assert
            Assert.IsTrue(outList.Count() == 10);
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

            int count = 1;
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine($"Cmb { count } : { combinationName } ");
                count++;
            }
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

            int count = 1;
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine($"Cmb { count } : { combinationName } ");
                count++;
            }
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

            int count = 1;
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine($"Cmb { count } : { combinationName } ");
                count++;
            }
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
            Assert.IsTrue(outList.Count() == 15);

            int count = 1;
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine($"Cmb { count } : { combinationName } ");
                count++;
            }
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

            int count = 1;
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine($"Cmb { count } : { combinationName } ");
                count++;
            }
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

            int count = 1;
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine($"Cmb { count } : { combinationName } ");
                count++;
            }
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

            int count = 1;
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine($"Cmb { count } : { combinationName } ");
                count++;
            }
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
            Assert.IsTrue(outList.Count() == 13);

            int count = 1;
            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine($"Cmb { count } : { combinationName } ");
                count++;
            }
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
            Combination combination5 = new Combination("cmb2", options1);

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


        [TestMethod]
        public void EqualsHashCode3()
        {
            // Arrange

            ASCE16CombinationsOptions options1 = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.LFRD);

            Combination combination1 = new Combination("cmb1", options1);
            Combination combination2 = new Combination("cmb1");

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

            // Assert / Act
            Assert.IsFalse(combination1.Equals(combination2));
            Assert.IsFalse(combination1.GetHashCode().Equals(combination2.GetHashCode()));

            Assert.IsFalse(combination2.Equals(combination1));
            Assert.IsFalse(combination2.GetHashCode().Equals(combination1.GetHashCode()));

        }

        [TestMethod]
        public void EqualsHashCode2()
        {
            // Arrange

            EN1990CombinationsOptions options1 = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateGeotechnical, StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, false);
            EN1990CombinationsOptions options2 = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, false);

            Combination combination1 = new Combination("cmb1", options1);
            Combination combination2 = new Combination("cmb1", options1);
            Combination combination3 = new Combination("cmb1", options2);
            Combination combination4 = new Combination("cmb1", options1);
            Combination combination5 = new Combination("cmb2", options1);

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

        [TestMethod]
        public void ScrambledEquals2()
        {
            // Arrange

            EN1990CombinationsOptions options1 = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateGeotechnical, StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, false);
            
            Combination combination1 = new Combination("cmb1", options1);
            Combination combination2 = new Combination("cmb1", options1);
            Combination combination3 = new Combination("cmb3", options1);
            Combination combination4 = new Combination("cmb4", options1);
            Combination combination5 = new Combination("cmb1", options1);
            Combination combination6 = new Combination("cmb1", options1);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseTypes.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseTypes.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseTypes.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);
            var lc62 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);

            combination1.AddLoadCaseCoefficient(lc1, 1);
            combination1.AddLoadCaseCoefficient(lc2, 2);
            combination1.AddLoadCaseCoefficient(lc4, 4);
            combination1.AddLoadCaseCoefficient(lc5, 5);
            combination1.AddLoadCaseCoefficient(lc6, 6);

            combination2.AddLoadCaseCoefficient(lc1, 1);
            combination2.AddLoadCaseCoefficient(lc2, 2);
            combination2.AddLoadCaseCoefficient(lc4, 4);
            combination2.AddLoadCaseCoefficient(lc5, 5);
            combination2.AddLoadCaseCoefficient(lc62, 6);

            combination3.AddLoadCaseCoefficient(lc1, 1);
            combination3.AddLoadCaseCoefficient(lc2, 2);
            combination3.AddLoadCaseCoefficient(lc4, 4);
            combination3.AddLoadCaseCoefficient(lc5, 5);
            combination3.AddLoadCaseCoefficient(lc6, 6);

            combination4.AddLoadCaseCoefficient(lc1, 1);
            combination4.AddLoadCaseCoefficient(lc2, 2);
            combination4.AddLoadCaseCoefficient(lc4, 4);
            combination4.AddLoadCaseCoefficient(lc5, 5);
            combination4.AddLoadCaseCoefficient(lc6, 6);

            combination5.AddLoadCaseCoefficient(lc1, 1);
            combination5.AddLoadCaseCoefficient(lc2, 2);
            combination5.AddLoadCaseCoefficient(lc4, 4);
            combination5.AddLoadCaseCoefficient(lc5, 5);
            combination5.AddLoadCaseCoefficient(lc6, 6);

            combination6.AddLoadCaseCoefficient(lc1, 1);
            combination6.AddLoadCaseCoefficient(lc2, 1);
            combination6.AddLoadCaseCoefficient(lc4, 1);
            combination6.AddLoadCaseCoefficient(lc5, 1);
            combination6.AddLoadCaseCoefficient(lc6, 1);


            List<double> loadCaseCoeff1 = combination1.GetLoadCaseCoefficients(out List<LoadCaseBase> loadCase1);
            List<double> loadCaseCoeff2 = combination2.GetLoadCaseCoefficients(out List<LoadCaseBase> loadCase2);
            List<double> loadCaseCoeff3 = combination3.GetLoadCaseCoefficients(out List<LoadCaseBase> loadCase3);
            List<double> loadCaseCoeff4 = combination4.GetLoadCaseCoefficients(out List<LoadCaseBase> loadCase4);
            List<double> loadCaseCoeff5 = combination5.GetLoadCaseCoefficients(out List<LoadCaseBase> loadCase5);
            List<double> loadCaseCoeff6 = combination6.GetLoadCaseCoefficients(out List<LoadCaseBase> loadCase6);

            List<Combination> list = new List<Combination>() { combination1, combination2, combination3, combination4, combination6 };

            // comb 1, comb 2, com 5 sono uguali
            // comb 3, comb 4 hanno nomi diversi
            // comb 6 ha coeff diversi

            // Assert / Act
            Assert.IsTrue(combination1.Equals(combination2));       
            Assert.IsFalse(combination1.Equals(combination3));      
            Assert.IsFalse(combination1.Equals(combination4));      
            Assert.IsTrue(combination1.Equals(combination5));       
            Assert.IsFalse(combination1.Equals(combination6));

            Assert.IsTrue(combination1.GetHashCode().Equals(combination2.GetHashCode()));       
            Assert.IsFalse(combination1.GetHashCode().Equals(combination3.GetHashCode()));      
            Assert.IsFalse(combination1.GetHashCode().Equals(combination4.GetHashCode()));
            
            Assert.IsTrue(loadCase1.SequenceEqual(loadCase2));
            Assert.IsTrue(loadCase1.SequenceEqual(loadCase3));
            Assert.IsTrue(loadCase1.SequenceEqual(loadCase4));
            Assert.IsTrue(loadCase1.SequenceEqual(loadCase5));
            Assert.IsFalse(loadCase1.SequenceEqual(loadCase6));

            Assert.IsTrue(loadCaseCoeff1.SequenceEqual(loadCaseCoeff2));
            Assert.IsTrue(loadCaseCoeff1.SequenceEqual(loadCaseCoeff3));
            Assert.IsTrue(loadCaseCoeff1.SequenceEqual(loadCaseCoeff4));
            Assert.IsTrue(loadCaseCoeff1.SequenceEqual(loadCaseCoeff5));
            Assert.IsFalse(loadCaseCoeff1.SequenceEqual(loadCaseCoeff6));

            Assert.IsTrue(list.Contains(combination1));
            Assert.IsTrue(list.Contains(combination2));
            Assert.IsTrue(list.Contains(combination3));
            Assert.IsTrue(list.Contains(combination4));
            Assert.IsTrue(list.Contains(combination5));
            Assert.IsTrue(list.Contains(combination6));
        }

        [TestMethod]
        public void ScrambledEquals()
        {
            // Arrange

            EN1990CombinationsOptions options1 = new EN1990CombinationsOptions(StandardEN1990.LimitStates.UltimateGeotechnical, StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, false);

            Combination combination1 = new Combination("cmb1", options1);
            Combination combination2 = new Combination("cmb1", options1);
            Combination combination3 = new Combination("cmb1", options1);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseTypes.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseTypes.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseTypes.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);
            var lc62 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);

            combination1.AddLoadCaseCoefficient(lc1, 1);
            combination1.AddLoadCaseCoefficient(lc2, 2);
            combination1.AddLoadCaseCoefficient(lc4, 4);
            combination1.AddLoadCaseCoefficient(lc5, 5);
            combination1.AddLoadCaseCoefficient(lc6, 6);

            combination2.AddLoadCaseCoefficient(lc1, 1);
            combination2.AddLoadCaseCoefficient(lc2, 2);
            combination2.AddLoadCaseCoefficient(lc4, 4);
            combination2.AddLoadCaseCoefficient(lc5, 5);
            combination2.AddLoadCaseCoefficient(lc62, 6);


            combination3.AddLoadCaseCoefficient(lc1, 1);
            combination3.AddLoadCaseCoefficient(lc2, 2);
            combination3.AddLoadCaseCoefficient(lc4, 4);
            combination3.AddLoadCaseCoefficient(lc5, 5);
            combination3.AddLoadCaseCoefficient(lc62, 6);

            List<double> loadCaseCoeff1 = combination1.GetLoadCaseCoefficients(out List<LoadCaseBase> loadCase1);
            List<double> loadCaseCoeff2 = combination2.GetLoadCaseCoefficients(out List<LoadCaseBase> loadCase2);


            // Assert / Act
            Assert.IsTrue(loadCase1.Contains(lc1));

            Assert.IsTrue(new List<Combination>() { combination1, combination2 }.Contains(combination3));


            Assert.IsTrue(combination1.Equals(combination2));
            Assert.IsTrue(combination1.GetHashCode().Equals(combination2.GetHashCode()));
            Assert.IsTrue(combination1.GetHashCode().Equals(combination2.GetHashCode()));
            Assert.IsTrue(loadCaseCoeff1.SequenceEqual(loadCaseCoeff2));
            Assert.IsTrue(loadCase1.SequenceEqual(loadCase2));
        }

        #endregion

    }
}