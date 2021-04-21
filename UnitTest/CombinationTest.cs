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
        public void CombinationContainsLoadCases()
        {
            // Arrange

            CombinationAsce combination = new CombinationAsce("cmb1", CombinationAsce.CombinationType.LFRD);
            
            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseType.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseType.SuperImposedDeadLoad);
            var lc3 = new LoadCase("LC3", LoadCase.LoadCaseType.ClimateSummer);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseType.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseType.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseType.Snow);

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

            CombinationAsce combination = new CombinationAsce("cmb1", CombinationAsce.CombinationType.LFRD);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseType.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseType.SuperImposedDeadLoad);
            var lc3 = new LoadCase("LC3", LoadCase.LoadCaseType.ClimateSummer);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseType.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseType.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseType.Snow);

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

            CombinationAsce combination = new CombinationAsce("cmb1", CombinationAsce.CombinationType.LFRD);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseType.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseType.SuperImposedDeadLoad);
            var lc3 = new LoadCase("LC3", LoadCase.LoadCaseType.ClimateSummer);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseType.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseType.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseType.Snow);

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

            CombinationAsce combination1 = new CombinationAsce("cmb1", CombinationAsce.CombinationType.LFRD);
            CombinationAsce combination2 = new CombinationAsce("cmb1", CombinationAsce.CombinationType.LFRD);
            CombinationAsce combination3 = new CombinationAsce("cmb1", CombinationAsce.CombinationType.ASD);

            CombinationAsce combination4 = new CombinationAsce("cmb1", CombinationAsce.CombinationType.LFRD);

            CombinationAsce combination5 = new CombinationAsce("cmb1", CombinationAsce.CombinationType.LFRD);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseType.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseType.SuperImposedDeadLoad);
            var lc3 = new LoadCase("LC3", LoadCase.LoadCaseType.ClimateSummer);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseType.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseType.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseType.Snow);

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
    }
}
