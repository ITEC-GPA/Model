using GPC.Model.Elements;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;
using GPC.Utilities.Serialization;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using GPC.Model.Standards;
using static System.Collections.Specialized.BitVector32;
using GPC.Model.Materials;
using GPC.Geometry;
using GPC.Model.Results;

namespace GeneralTest
{
    [TestClass]
    public class SerializationTest : UnitTestBase
    {
        private bool SerializationClassesCommonAsserts(object objToTest)
        {
            bool check = true;

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, objToTest);
                ms.Position = 0;

                var oggettoDeserializzato = formatter.Deserialize(ms);

                if (objToTest == oggettoDeserializzato)
                {
                    Console.WriteLine($"Class {objToTest.ToString()} is serializable");
                }
                else
                {
                    Console.WriteLine($"Warning: Class {objToTest.ToString()} is not serializable");
                    check = false;
                }
            }
            return check;
        }

        /// <summary>
        /// Testa che tutte le classi nell'assembly siano abbiano l'attributo [Serializable]
        /// </summary>
        [TestMethod]
        public void SerializableAttributeTest()
        {
            var assemblyName = "GPCModel";
            var nameSpace = "GPC.Model";

            var assembly = Assembly.Load(assemblyName);
            var classes = assembly.GetTypes().Where(a => a.IsClass && a.Namespace != null && a.Namespace.Contains(nameSpace)).ToList();

            foreach (var cl in classes)
            {
                if (!cl.IsSerializable)
                    Assert.Fail($"Class {cl.Name} is not serializable");
            }
        }

		#region Standards

		/// <summary>
		/// Testa che tutte le classi nell'assembly siano abbiano l'attributo [Serializable]
		/// </summary>
		[TestMethod]
        public void SerializableTest1()
        {
            GhostElement ghostElement = new GhostElement();

            var bytes = Serialization.SerializeToBytes(ghostElement);

            var a = Serialization.DeserializeFromBytes(bytes);

            Assert.IsTrue(ghostElement.Equals(a));
        }

        [TestMethod]
        public void StandardCopSuos2011Test()
        {
            bool check = true;

            StandardCopSuos2011 s = new StandardCopSuos2011();
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                StandardCopSuos2011 oggettoDeserializzato = (StandardCopSuos2011)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.GammaM1 != oggettoDeserializzato.GammaM1 ||
                        s.GammaM2 != oggettoDeserializzato.GammaM2)
                        check = false;                    
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void StandardEN16612Test()
        {
            bool check = true;

            StandardEN16612 s = new StandardEN16612();
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                StandardEN16612 oggettoDeserializzato = (StandardEN16612)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Psi0ClimateSummerDeltaP != oggettoDeserializzato.Psi0ClimateSummerDeltaP ||
                        s.Psi0ClimateSummerDeltaT != oggettoDeserializzato.Psi0ClimateSummerDeltaT ||
                        s.Psi0ClimateWinterDeltaP != oggettoDeserializzato.Psi0ClimateWinterDeltaP ||
                        s.Psi0ClimateWinterDeltaT != oggettoDeserializzato.Psi0ClimateWinterDeltaT ||
                        s.Psi1ClimateSummerDeltaP != oggettoDeserializzato.Psi1ClimateSummerDeltaP ||
                        s.Psi1ClimateSummerDeltaT != oggettoDeserializzato.Psi1ClimateSummerDeltaT ||
                        s.Psi1ClimateWinterDeltaP != oggettoDeserializzato.Psi1ClimateWinterDeltaP ||
                        s.Psi1ClimateWinterDeltaT != oggettoDeserializzato.Psi1ClimateWinterDeltaT ||
                        s.Psi2ClimateSummerDeltaP != oggettoDeserializzato.Psi2ClimateSummerDeltaP ||
                        s.Psi2ClimateSummerDeltaT != oggettoDeserializzato.Psi2ClimateSummerDeltaT ||
                        s.Psi2ClimateWinterDeltaP != oggettoDeserializzato.Psi2ClimateWinterDeltaP ||
                        s.Psi2ClimateWinterDeltaT != oggettoDeserializzato.Psi2ClimateWinterDeltaT)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void StandardEN1990Test()
        {
            bool check = true;

            StandardEN1990 s = new StandardEN1990();
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                StandardEN1990 oggettoDeserializzato = (StandardEN1990)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.GammaGFavourableSetA != oggettoDeserializzato.GammaGFavourableSetA ||
                        s.GammaGUnfavourableSetA != oggettoDeserializzato.GammaGUnfavourableSetA ||
                        s.GammaGFavourableSetB != oggettoDeserializzato.GammaGFavourableSetB ||
                        s.GammaGUnfavourableSetB != oggettoDeserializzato.GammaGUnfavourableSetB ||
                        s.GammaGFavourableSetC != oggettoDeserializzato.GammaGFavourableSetC ||
                        s.GammaGUnfavourableSetC != oggettoDeserializzato.GammaGUnfavourableSetC ||

                        s.GammaQFavourableSetA != oggettoDeserializzato.GammaQFavourableSetA ||
                        s.GammaQUnfavourableSetA != oggettoDeserializzato.GammaQUnfavourableSetA ||
                        s.GammaQFavourableSetB != oggettoDeserializzato.GammaQFavourableSetB ||
                        s.GammaQUnfavourableSetB != oggettoDeserializzato.GammaQUnfavourableSetB ||
                        s.GammaQFavourableSetC != oggettoDeserializzato.GammaQFavourableSetC ||
                        s.GammaQUnfavourableSetC != oggettoDeserializzato.GammaQUnfavourableSetC ||
                        
                        s.GammaPFavourableSetA != oggettoDeserializzato.GammaPFavourableSetA ||
                        s.GammaPUnfavourableSetA != oggettoDeserializzato.GammaPUnfavourableSetA ||
                        s.GammaPFavourableSetB != oggettoDeserializzato.GammaPFavourableSetB ||
                        s.GammaPUnfavourableSetB != oggettoDeserializzato.GammaPUnfavourableSetB ||
                        s.GammaPFavourableSetC != oggettoDeserializzato.GammaPFavourableSetC ||
                        s.GammaPUnfavourableSetC != oggettoDeserializzato.GammaPUnfavourableSetC ||

                        s.ImposedLoadPsi0CategoryA != oggettoDeserializzato.ImposedLoadPsi0CategoryA ||
                        s.ImposedLoadPsi0CategoryB != oggettoDeserializzato.ImposedLoadPsi0CategoryB ||
                        s.ImposedLoadPsi0CategoryC != oggettoDeserializzato.ImposedLoadPsi0CategoryC ||
                        s.ImposedLoadPsi0CategoryD != oggettoDeserializzato.ImposedLoadPsi0CategoryD ||
                        s.ImposedLoadPsi0CategoryE != oggettoDeserializzato.ImposedLoadPsi0CategoryE ||
                        s.ImposedLoadPsi0CategoryF != oggettoDeserializzato.ImposedLoadPsi0CategoryF ||
                        s.ImposedLoadPsi0CategoryG != oggettoDeserializzato.ImposedLoadPsi0CategoryG ||
                        s.ImposedLoadPsi0CategoryH != oggettoDeserializzato.ImposedLoadPsi0CategoryH ||
                        
                        s.ImposedLoadPsi1CategoryA != oggettoDeserializzato.ImposedLoadPsi1CategoryA ||
                        s.ImposedLoadPsi1CategoryB != oggettoDeserializzato.ImposedLoadPsi1CategoryB ||
                        s.ImposedLoadPsi1CategoryC != oggettoDeserializzato.ImposedLoadPsi1CategoryC ||
                        s.ImposedLoadPsi1CategoryD != oggettoDeserializzato.ImposedLoadPsi1CategoryD ||
                        s.ImposedLoadPsi1CategoryE != oggettoDeserializzato.ImposedLoadPsi1CategoryE ||
                        s.ImposedLoadPsi1CategoryF != oggettoDeserializzato.ImposedLoadPsi1CategoryF ||
                        s.ImposedLoadPsi1CategoryG != oggettoDeserializzato.ImposedLoadPsi1CategoryG ||
                        s.ImposedLoadPsi1CategoryH != oggettoDeserializzato.ImposedLoadPsi1CategoryH ||
                        
                        s.ImposedLoadPsi2CategoryA != oggettoDeserializzato.ImposedLoadPsi2CategoryA ||
                        s.ImposedLoadPsi2CategoryB != oggettoDeserializzato.ImposedLoadPsi2CategoryB ||
                        s.ImposedLoadPsi2CategoryC != oggettoDeserializzato.ImposedLoadPsi2CategoryC ||
                        s.ImposedLoadPsi2CategoryD != oggettoDeserializzato.ImposedLoadPsi2CategoryD ||
                        s.ImposedLoadPsi2CategoryE != oggettoDeserializzato.ImposedLoadPsi2CategoryE ||
                        s.ImposedLoadPsi2CategoryF != oggettoDeserializzato.ImposedLoadPsi2CategoryF ||
                        s.ImposedLoadPsi2CategoryG != oggettoDeserializzato.ImposedLoadPsi2CategoryG ||
                        s.ImposedLoadPsi2CategoryH != oggettoDeserializzato.ImposedLoadPsi2CategoryH ||
                        
                        s.Psi0SnowHighAltitude != oggettoDeserializzato.Psi0SnowHighAltitude ||
                        s.Psi0SnowLowAltitude  != oggettoDeserializzato.Psi0SnowLowAltitude  ||
                        s.Psi1SnowHighAltitude != oggettoDeserializzato.Psi1SnowHighAltitude ||
                        s.Psi1SnowLowAltitude  != oggettoDeserializzato.Psi1SnowLowAltitude  ||
                        s.Psi2SnowHighAltitude != oggettoDeserializzato.Psi2SnowHighAltitude ||
                        s.Psi2SnowLowAltitude != oggettoDeserializzato.Psi2SnowLowAltitude ||

                        s.Psi0Wind != oggettoDeserializzato.Psi0Wind ||
                        s.Psi1Wind != oggettoDeserializzato.Psi1Wind ||
                        s.Psi2Wind != oggettoDeserializzato.Psi2Wind ||
                        
                        s.Psi0Temperature != oggettoDeserializzato.Psi0Temperature ||
                        s.Psi1Temperature != oggettoDeserializzato.Psi1Temperature ||
                        s.Psi2Temperature != oggettoDeserializzato.Psi2Temperature)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void StandardEN1993p11Test()
        {
            bool check = true;

            StandardEN1993p11 s = new StandardEN1993p11();
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                StandardEN1993p11 oggettoDeserializzato = (StandardEN1993p11)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.GammaM0 != oggettoDeserializzato.GammaM0 ||
                        s.GammaM1 != oggettoDeserializzato.GammaM1 ||
                        s.GammaM2 != oggettoDeserializzato.GammaM2 ||
                        s.NShearBucklingLowGradeOfSteel  != oggettoDeserializzato.NShearBucklingLowGradeOfSteel  ||
                        s.NShearBucklingHighGradeOfSteel != oggettoDeserializzato.NShearBucklingHighGradeOfSteel ||
                        s.AlphaImperfectionFactorForCurveA0 != oggettoDeserializzato.AlphaImperfectionFactorForCurveA0 ||
                        s.AlphaImperfectionFactorForCurveA != oggettoDeserializzato.AlphaImperfectionFactorForCurveA ||
                        s.AlphaImperfectionFactorForCurveB != oggettoDeserializzato.AlphaImperfectionFactorForCurveB ||
                        s.AlphaImperfectionFactorForCurveC != oggettoDeserializzato.AlphaImperfectionFactorForCurveC ||
                        s.AlphaImperfectionFactorForCurveD != oggettoDeserializzato.AlphaImperfectionFactorForCurveD ||
                        s.AlphaLTImperfectionFactorForCurveA != oggettoDeserializzato.AlphaLTImperfectionFactorForCurveA ||
                        s.AlphaLTImperfectionFactorForCurveB != oggettoDeserializzato.AlphaLTImperfectionFactorForCurveB ||
                        s.AlphaLTImperfectionFactorForCurveC != oggettoDeserializzato.AlphaLTImperfectionFactorForCurveC ||
                        s.AlphaLTImperfectionFactorForCurveD != oggettoDeserializzato.AlphaLTImperfectionFactorForCurveD ||
                        s.BetaForLateralTorsionalBuckling != oggettoDeserializzato.BetaForLateralTorsionalBuckling ||
                        s.LambdaLT0ForLateralTorsionalBuckling != oggettoDeserializzato.LambdaLT0ForLateralTorsionalBuckling ||
                        s.BetaForLateralTorsionalBucklingMod != oggettoDeserializzato.BetaForLateralTorsionalBucklingMod ||
                        s.LambdaLT0ForLateralTorsionalBucklingMod != oggettoDeserializzato.LambdaLT0ForLateralTorsionalBucklingMod)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void StandardUNIEN1993p11Test()
        {
            bool check = true;

            StandardUNIEN1993p11 s = new StandardUNIEN1993p11();
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                StandardUNIEN1993p11 oggettoDeserializzato = (StandardUNIEN1993p11)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.GammaM0 != oggettoDeserializzato.GammaM0 ||
                        s.GammaM1 != oggettoDeserializzato.GammaM1 ||
                        s.GammaM2 != oggettoDeserializzato.GammaM2 ||
                        s.NShearBucklingLowGradeOfSteel != oggettoDeserializzato.NShearBucklingLowGradeOfSteel ||
                        s.NShearBucklingHighGradeOfSteel != oggettoDeserializzato.NShearBucklingHighGradeOfSteel ||
                        s.AlphaImperfectionFactorForCurveA0 != oggettoDeserializzato.AlphaImperfectionFactorForCurveA0 ||
                        s.AlphaImperfectionFactorForCurveA != oggettoDeserializzato.AlphaImperfectionFactorForCurveA ||
                        s.AlphaImperfectionFactorForCurveB != oggettoDeserializzato.AlphaImperfectionFactorForCurveB ||
                        s.AlphaImperfectionFactorForCurveC != oggettoDeserializzato.AlphaImperfectionFactorForCurveC ||
                        s.AlphaImperfectionFactorForCurveD != oggettoDeserializzato.AlphaImperfectionFactorForCurveD ||
                        s.AlphaLTImperfectionFactorForCurveA != oggettoDeserializzato.AlphaLTImperfectionFactorForCurveA ||
                        s.AlphaLTImperfectionFactorForCurveB != oggettoDeserializzato.AlphaLTImperfectionFactorForCurveB ||
                        s.AlphaLTImperfectionFactorForCurveC != oggettoDeserializzato.AlphaLTImperfectionFactorForCurveC ||
                        s.AlphaLTImperfectionFactorForCurveD != oggettoDeserializzato.AlphaLTImperfectionFactorForCurveD ||
                        s.BetaForLateralTorsionalBuckling != oggettoDeserializzato.BetaForLateralTorsionalBuckling ||
                        s.LambdaLT0ForLateralTorsionalBuckling != oggettoDeserializzato.LambdaLT0ForLateralTorsionalBuckling ||
                        s.BetaForLateralTorsionalBucklingMod != oggettoDeserializzato.BetaForLateralTorsionalBucklingMod ||
                        s.LambdaLT0ForLateralTorsionalBucklingMod != oggettoDeserializzato.LambdaLT0ForLateralTorsionalBucklingMod)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void StandardModelCode2010Test()
        {
            bool check = true;

            StandardModelCode2010 s = new StandardModelCode2010();
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                StandardModelCode2010 oggettoDeserializzato = (StandardModelCode2010)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.GammaC != oggettoDeserializzato.GammaC ||
                        s.GammaCAccidental != oggettoDeserializzato.GammaCAccidental ||
                        s.GammaCE != oggettoDeserializzato.GammaCE ||
                        s.GammaS != oggettoDeserializzato.GammaS ||
                        s.GammaSAccidental != oggettoDeserializzato.GammaSAccidental ||
                        s.GammaSPrestress != oggettoDeserializzato.GammaSPrestress ||
                        s.GammaSPrestressAccidental != oggettoDeserializzato.GammaSPrestressAccidental ||
                        s.AlphaCC != oggettoDeserializzato.AlphaCC ||
                        s.AlphaCT != oggettoDeserializzato.AlphaCT ||
                        s.GammaF != oggettoDeserializzato.GammaF ||
                        s.SteelCoefficientStrainTension != oggettoDeserializzato.SteelCoefficientStrainTension)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void StandardCNR204Test()
        {
            bool check = true;

            StandardCNR204 s = new StandardCNR204();
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                StandardCNR204 oggettoDeserializzato = (StandardCNR204)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.GammaC != oggettoDeserializzato.GammaC ||
                        s.GammaCAccidental != oggettoDeserializzato.GammaCAccidental ||
                        s.GammaCE != oggettoDeserializzato.GammaCE ||
                        s.GammaS != oggettoDeserializzato.GammaS ||
                        s.GammaSAccidental != oggettoDeserializzato.GammaSAccidental ||
                        s.GammaSPrestress != oggettoDeserializzato.GammaSPrestress ||
                        s.GammaSPrestressAccidental != oggettoDeserializzato.GammaSPrestressAccidental ||
                        s.AlphaCC != oggettoDeserializzato.AlphaCC ||
                        s.AlphaCT != oggettoDeserializzato.AlphaCT ||
                        s.GammaF != oggettoDeserializzato.GammaF ||
                        s.SteelCoefficientStrainTension != oggettoDeserializzato.SteelCoefficientStrainTension)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void StandardNTC2018ConcreteTest()
        {
            bool check = true;

            StandardNTC2018Concrete s = new StandardNTC2018Concrete();
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                StandardNTC2018Concrete oggettoDeserializzato = (StandardNTC2018Concrete)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.GammaC != oggettoDeserializzato.GammaC ||
                        s.GammaCAccidental != oggettoDeserializzato.GammaCAccidental ||
                        s.GammaCE != oggettoDeserializzato.GammaCE ||
                        s.GammaS != oggettoDeserializzato.GammaS ||
                        s.GammaSAccidental != oggettoDeserializzato.GammaSAccidental ||
                        s.GammaSPrestress != oggettoDeserializzato.GammaSPrestress ||
                        s.GammaSPrestressAccidental != oggettoDeserializzato.GammaSPrestressAccidental ||
                        s.AlphaCC != oggettoDeserializzato.AlphaCC ||
                        s.AlphaCT != oggettoDeserializzato.AlphaCT ||
                        s.GammaF != oggettoDeserializzato.GammaF ||
                        s.SteelCoefficientStrainTension != oggettoDeserializzato.SteelCoefficientStrainTension)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void StandardEN1992p11Test()
        {
            bool check = true;

            StandardEN1992p11 s = new StandardEN1992p11();
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                StandardEN1992p11 oggettoDeserializzato = (StandardEN1992p11)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.GammaC != oggettoDeserializzato.GammaC ||
                        s.GammaCAccidental != oggettoDeserializzato.GammaCAccidental ||
                        s.GammaCE != oggettoDeserializzato.GammaCE ||
                        s.GammaS != oggettoDeserializzato.GammaS ||
                        s.GammaSAccidental != oggettoDeserializzato.GammaSAccidental ||
                        s.GammaSPrestress != oggettoDeserializzato.GammaSPrestress ||
                        s.GammaSPrestressAccidental != oggettoDeserializzato.GammaSPrestressAccidental ||
                        s.AlphaCC != oggettoDeserializzato.AlphaCC ||
                        s.AlphaCT != oggettoDeserializzato.AlphaCT ||
                        s.GammaF != oggettoDeserializzato.GammaF ||
                        s.SteelCoefficientStrainTension != oggettoDeserializzato.SteelCoefficientStrainTension)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void StandardUNIEn1992p11Test()
        {
            bool check = true;

            StandardUNIEn1992p11 s = new StandardUNIEn1992p11();
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                StandardUNIEn1992p11 oggettoDeserializzato = (StandardUNIEn1992p11)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.GammaC != oggettoDeserializzato.GammaC ||
                        s.GammaCAccidental != oggettoDeserializzato.GammaCAccidental ||
                        s.GammaCE != oggettoDeserializzato.GammaCE ||
                        s.GammaS != oggettoDeserializzato.GammaS ||
                        s.GammaSAccidental != oggettoDeserializzato.GammaSAccidental ||
                        s.GammaSPrestress != oggettoDeserializzato.GammaSPrestress ||
                        s.GammaSPrestressAccidental != oggettoDeserializzato.GammaSPrestressAccidental ||
                        s.AlphaCC != oggettoDeserializzato.AlphaCC ||
                        s.AlphaCT != oggettoDeserializzato.AlphaCT ||
                        s.GammaF != oggettoDeserializzato.GammaF ||
                        s.SteelCoefficientStrainTension != oggettoDeserializzato.SteelCoefficientStrainTension)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

		#endregion

		#region Sections

		[TestMethod]
        public void SectionTest()
        {
            bool check = true;

            GPC.Model.Sections.Section s = new GPC.Model.Sections.Section(
                new Material("test", 10, 0.2, 20, 5), 50, 200, 300, 500, 40, Point3d.Origin, Point3d.Origin, 0.2, "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                GPC.Model.Sections.Section oggettoDeserializzato = (GPC.Model.Sections.Section)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Material != oggettoDeserializzato.Material ||
                        s.Area != oggettoDeserializzato.Area ||
                        s.Jt != oggettoDeserializzato.Jt ||
                        s.Jw != oggettoDeserializzato.Jw ||
                        s.Jxx != oggettoDeserializzato.Jxx ||
                        s.Jyy != oggettoDeserializzato.Jyy ||
                        s.Jxy != oggettoDeserializzato.Jxy ||
                        s.Jp != oggettoDeserializzato.Jp ||
                        s.J11 != oggettoDeserializzato.J11 ||
                        s.J22 != oggettoDeserializzato.J22 ||
                        s.Wpl1 != oggettoDeserializzato.Wpl1 ||
                        s.Wpl2 != oggettoDeserializzato.Wpl2 ||
                        s.Wel1Min != oggettoDeserializzato.Wel1Min ||
                        s.Wel1Max != oggettoDeserializzato.Wel1Max ||
                        s.Wel2Max != oggettoDeserializzato.Wel2Max ||
                        s.Wel2Min != oggettoDeserializzato.Wel2Min ||
                        s.WelXMin != oggettoDeserializzato.WelXMin ||
                        s.WelXMax != oggettoDeserializzato.WelXMax ||
                        s.WelYMin != oggettoDeserializzato.WelYMin ||
                        s.WelYMax != oggettoDeserializzato.WelYMax ||
                        s.WplX != oggettoDeserializzato.WplX ||
                        s.WplY != oggettoDeserializzato.WplY ||
                        s.Centroid != oggettoDeserializzato.Centroid ||
                        s.ShearCenter != oggettoDeserializzato.ShearCenter ||
                        s.AngleX1 != oggettoDeserializzato.AngleX1 ||
                        s.IsSymmetricAlongXLocalAxis != oggettoDeserializzato.IsSymmetricAlongXLocalAxis ||
                        s.IsSymmetricAlongYLocalAxis != oggettoDeserializzato.IsSymmetricAlongYLocalAxis ||
                        s.Shape != oggettoDeserializzato.Shape ||
                        s.Mesh != oggettoDeserializzato.Mesh)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        #endregion

        #region Materials

        [TestMethod]
        public void MaterialTest()
        {
            bool check = true;

            Material m = new Material("test", 10, 0.2, 20, 5);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                Material oggettoDeserializzato = (Material)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.AlfaThermalExpansion != oggettoDeserializzato.AlfaThermalExpansion ||
                        m.Density != oggettoDeserializzato.Density ||
                        m.E != oggettoDeserializzato.E ||
                        m.Name != oggettoDeserializzato.Name ||
                        m.Guid != oggettoDeserializzato.Guid ||
                        m.Ni != oggettoDeserializzato.Ni)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {m.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void ConcreteMaterialModelCode2010FRCTest()
        {
            bool check = true;

            ConcreteMaterialModelCode2010FRC m = new ConcreteMaterialModelCode2010FRC("test", -25, 
                ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 1, 2, 0.1, 0.2, 
                ConcreteMaterialModelCode2010.TensionStressStrainDiagrams.Bilinear, 0.2, 20, 5, 
                ConcreteMaterialModelCode2010.CementType.ClassN);

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ConcreteMaterialModelCode2010FRC oggettoDeserializzato = (ConcreteMaterialModelCode2010FRC)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.AlfaThermalExpansion != oggettoDeserializzato.AlfaThermalExpansion ||
                        m.Density != oggettoDeserializzato.Density ||
                        m.E != oggettoDeserializzato.E ||
                        m.Name != oggettoDeserializzato.Name ||
                        m.Guid != oggettoDeserializzato.Guid ||
                        m.Fck != oggettoDeserializzato.Fck ||
                        m.Fctk != oggettoDeserializzato.Fctk ||
                        m.Fctu != oggettoDeserializzato.Fctu ||
                        m.StrainYCompression != oggettoDeserializzato.StrainYCompression ||
                        m.StrainUCompression != oggettoDeserializzato.StrainUCompression ||
                        m.StrainYTension != oggettoDeserializzato.StrainYTension ||
                        m.StrainUTension != oggettoDeserializzato.StrainUTension ||
                        m.CompressionStressStrainDiagram != oggettoDeserializzato.CompressionStressStrainDiagram ||
                        m.TensionStressStrainDiagram != oggettoDeserializzato.TensionStressStrainDiagram)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {m.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void ConcreteMaterialEN1992Test()
        {
            bool check = true;

            ConcreteMaterialEN1992 m = new ConcreteMaterialEN1992("test", -25,
                ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.2, 20, 5,
                ConcreteMaterialModelCode2010.CementType.ClassN);

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ConcreteMaterialEN1992 oggettoDeserializzato = (ConcreteMaterialEN1992)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.AlfaThermalExpansion != oggettoDeserializzato.AlfaThermalExpansion ||
                        m.Density != oggettoDeserializzato.Density ||
                        m.E != oggettoDeserializzato.E ||
                        m.Name != oggettoDeserializzato.Name ||
                        m.Guid != oggettoDeserializzato.Guid ||
                        m.Fck != oggettoDeserializzato.Fck ||
                        m.Fctk != oggettoDeserializzato.Fctk ||
                        m.Fctu != oggettoDeserializzato.Fctu ||
                        m.StrainYCompression != oggettoDeserializzato.StrainYCompression ||
                        m.StrainUCompression != oggettoDeserializzato.StrainUCompression ||
                        m.StrainYTension != oggettoDeserializzato.StrainYTension ||
                        m.StrainUTension != oggettoDeserializzato.StrainUTension ||
                        m.CompressionStressStrainDiagram != oggettoDeserializzato.CompressionStressStrainDiagram ||
                        m.TensionStressStrainDiagram != oggettoDeserializzato.TensionStressStrainDiagram)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {m.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SteelMaterialTest()
        {
            bool check = true;

            SteelMaterial m = new SteelMaterial("test", 10, 15, 20, 30, 0.2, 20, 5);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SteelMaterial oggettoDeserializzato = (SteelMaterial)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.AlfaThermalExpansion != oggettoDeserializzato.AlfaThermalExpansion ||
                        m.Density != oggettoDeserializzato.Density ||
                        m.E != oggettoDeserializzato.E ||
                        m.Name != oggettoDeserializzato.Name ||
                        m.Guid != oggettoDeserializzato.Guid ||
                        m.Fyk != oggettoDeserializzato.Fyk ||
                        m.Fu != oggettoDeserializzato.Fu ||
                        m.StrainU != oggettoDeserializzato.StrainU ||
                        m.Ni != oggettoDeserializzato.Ni)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {m.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void RebarMaterialTest()
        {
            bool check = true;

            RebarMaterial m = new RebarMaterial("test", 10, 15, 20, 30, 0.2, 20, 5);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                RebarMaterial oggettoDeserializzato = (RebarMaterial)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.AlfaThermalExpansion != oggettoDeserializzato.AlfaThermalExpansion ||
                        m.Density != oggettoDeserializzato.Density ||
                        m.E != oggettoDeserializzato.E ||
                        m.Name != oggettoDeserializzato.Name ||
                        m.Guid != oggettoDeserializzato.Guid ||
                        m.Fyk != oggettoDeserializzato.Fyk ||
                        m.Fu != oggettoDeserializzato.Fu ||
                        m.StrainU != oggettoDeserializzato.StrainU ||
                        m.Ni != oggettoDeserializzato.Ni)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {m.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        #endregion

        #region Result

        [TestMethod]
        public void ResultBeamForcesTest()
        {
            bool check = true;

            ResultBeamForces m = new ResultBeamForces(1, 2, 3, 4, 5, 6, CoordinateSystem.Global, 1);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ResultBeamForces oggettoDeserializzato = (ResultBeamForces)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.N != oggettoDeserializzato.N ||
                        m.V1 != oggettoDeserializzato.V1 ||
                        m.V2 != oggettoDeserializzato.V2 ||
                        m.T != oggettoDeserializzato.T ||
                        m.M1 != oggettoDeserializzato.M1 ||
                        m.CoordinateSystem != oggettoDeserializzato.CoordinateSystem ||
                        m.Id != oggettoDeserializzato.Id ||
                        m.M2 != oggettoDeserializzato.M2)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m.ToString()} is serializable");
            else
                Console.WriteLine($"Warning: Class {m.ToString()} is not serializable");

            Assert.IsTrue(check);
        }

        #endregion
    }
}