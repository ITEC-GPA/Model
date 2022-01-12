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
using GPC.Model.LoadCases;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Sections.Steel;

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
                    Console.WriteLine($"Class {objToTest} is serializable");
                }
                else
                {
                    Console.WriteLine($"Warning: Class {objToTest} is not serializable");
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
        public void Standard_CopSuos2011Test()
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
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Standard_EN16612Test()
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
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Standard_EN1990Test()
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
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Standard_EN1993p11Test()
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
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Standard_UNIEN1993p11Test()
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
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Standard_ModelCode2010Test()
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
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Standard_CNR204Test()
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
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Standard_NTC2018ConcreteTest()
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
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Standard_EN1992p11Test()
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
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Standard_UNIEn1992p11Test()
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
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

		#endregion

		#region Sections

		#region Generic Sections

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
                        s.Mesh != oggettoDeserializzato.Mesh)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_CircularTest()
        {
            bool check = true;

            SectionCircular s = new SectionCircular(10, new Material("test", 10, 0.2, 20, 5), "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SectionCircular oggettoDeserializzato = (SectionCircular)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Material != oggettoDeserializzato.Material ||
                        s.Diameter != oggettoDeserializzato.Diameter ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_CHSTest()
        {
            bool check = true;

            SectionCHS s = new SectionCHS(10, 2, new Material("test", 10, 0.2, 20, 5), "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SectionCHS oggettoDeserializzato = (SectionCHS)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Material != oggettoDeserializzato.Material ||
                        s.Diameter != oggettoDeserializzato.Diameter ||
                        s.Thickness != oggettoDeserializzato.Thickness ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_CTest()
        {
            bool check = true;

            SectionC s = new SectionC(200, 4, 100, 5, 100, 5, new Material("test", 10, 0.2, 20, 5), "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SectionC oggettoDeserializzato = (SectionC)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Height != oggettoDeserializzato.Height ||
                        s.ThicknessWeb != oggettoDeserializzato.ThicknessWeb ||
                        s.LengthBottom != oggettoDeserializzato.LengthBottom ||
                        s.ThicknessBottom != oggettoDeserializzato.ThicknessBottom ||
                        s.LengthTop != oggettoDeserializzato.LengthTop ||
                        s.LengthTop != oggettoDeserializzato.LengthTop ||
                        s.ThicknessTop != oggettoDeserializzato.ThicknessTop ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_HTest()
        {
            bool check = true;

            SectionH s = new SectionH(200, 4, 100, 5, 100, 5, new Material("test", 10, 0.2, 20, 5), "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SectionH oggettoDeserializzato = (SectionH)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Height != oggettoDeserializzato.Height ||
                        s.ThicknessWeb != oggettoDeserializzato.ThicknessWeb ||
                        s.LenghtBottomFlange != oggettoDeserializzato.LenghtBottomFlange ||
                        s.LenghtTopFlange != oggettoDeserializzato.LenghtTopFlange ||
                        s.ThicknessTopFlange != oggettoDeserializzato.ThicknessTopFlange ||
                        s.ThicknessBottomFlange != oggettoDeserializzato.ThicknessBottomFlange ||
                        s.ThicknessWeb != oggettoDeserializzato.ThicknessWeb ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_LTest()
        {
            bool check = true;

            SectionL s = new SectionL(200, 4, 100, 5, new Material("test", 10, 0.2, 20, 5), "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SectionL oggettoDeserializzato = (SectionL)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.HorizontalLegLength != oggettoDeserializzato.HorizontalLegLength ||
                        s.HorizontalLegThickness != oggettoDeserializzato.HorizontalLegThickness ||
                        s.VerticalLegLength != oggettoDeserializzato.VerticalLegLength ||
                        s.VerticalLegThickness != oggettoDeserializzato.VerticalLegThickness ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_RectangularTest()
        {
            bool check = true;

            SectionRectangular s = new SectionRectangular(200, 4, new Material("test", 10, 0.2, 20, 5), "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SectionRectangular oggettoDeserializzato = (SectionRectangular)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Height != oggettoDeserializzato.Height ||
                        s.Width != oggettoDeserializzato.Width ||
                        s.Angle != oggettoDeserializzato.Angle ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_RHSTest()
        {
            bool check = true;

            SectionRHS s = new SectionRHS(300, 200, 5, 5, 5, 5, new Material("test", 10, 0.2, 20, 5), "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SectionRHS oggettoDeserializzato = (SectionRHS)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Height != oggettoDeserializzato.Height ||
                        s.Base != oggettoDeserializzato.Base ||
                        s.ThicknessTop != oggettoDeserializzato.ThicknessTop ||
                        s.ThicknessBottom != oggettoDeserializzato.ThicknessBottom ||
                        s.ThicknessWebLeft != oggettoDeserializzato.ThicknessWebLeft ||
                        s.ThicknessWebRight != oggettoDeserializzato.ThicknessWebRight ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_TTest()
        {
            bool check = true;

            SectionT s = new SectionT(300, 200, 5, 5, new Material("test", 10, 0.2, 20, 5), "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SectionT oggettoDeserializzato = (SectionT)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Height != oggettoDeserializzato.Height ||
                        s.ThicknessWeb != oggettoDeserializzato.ThicknessWeb ||
                        s.ThicknessFlange != oggettoDeserializzato.ThicknessFlange ||
                        s.LenghtFlange != oggettoDeserializzato.LenghtFlange ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

		#endregion

		#region Concrete Sections

		[TestMethod]
        public void Section_ShapeExTest()
        {
            bool check = true;

            ShapeEx s = new ShapeEx(new Polygon2d(500), new Material("test", 10, 0.2, 20, 5), new Polygon2d[] {new Polygon2d(250)}, null);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ShapeEx oggettoDeserializzato = (ShapeEx)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Shape != oggettoDeserializzato.Shape ||
                        s.Material != oggettoDeserializzato.Material)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Concrete_CircularTest()
        {
            bool check = true;

            ConcreteSectionCircular s = new ConcreteSectionCircular(10, ConcreteMaterialEN1992.C25_30, "section");
            s.AddRebars(new ReinforcedConcreteRebar[] {
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), Point2d.Origin),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(10, 10)),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(-10, -10))});

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ConcreteSectionCircular oggettoDeserializzato = (ConcreteSectionCircular)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Rebars.Count() != oggettoDeserializzato.Rebars.Count())
                        check = false;
                    for (int i = 0; i < s.Rebars.Count(); i++)
                        if (s.Rebars.ToArray()[i] != oggettoDeserializzato.Rebars.ToArray()[i])
                            check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Concrete_CHSTest()
        {
            bool check = true;

            ConcreteSectionCHS s = new ConcreteSectionCHS(10, 2, ConcreteMaterialEN1992.C25_30, "section");
            s.AddRebars(new ReinforcedConcreteRebar[] {
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), Point2d.Origin),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(10, 10)),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(-10, -10))});

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ConcreteSectionCHS oggettoDeserializzato = (ConcreteSectionCHS)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Rebars.Count() != oggettoDeserializzato.Rebars.Count())
                        check = false;
                    for (int i = 0; i < s.Rebars.Count(); i++)
                        if (s.Rebars.ToArray()[i] != oggettoDeserializzato.Rebars.ToArray()[i])
                            check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Concrete_TTest()
        {
            bool check = true;

            ConcreteSectionT s = new ConcreteSectionT(500, 600, 50, 40, ConcreteMaterialEN1992.C25_30, "section"); 
            s.AddRebars(new ReinforcedConcreteRebar[] {
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), Point2d.Origin),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(10, 10)),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(-10, -10))});

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ConcreteSectionT oggettoDeserializzato = (ConcreteSectionT)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Rebars.Count() != oggettoDeserializzato.Rebars.Count())
                        check = false;
                    for (int i = 0; i < s.Rebars.Count(); i++)
                        if (s.Rebars.ToArray()[i] != oggettoDeserializzato.Rebars.ToArray()[i])
                            check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Concrete_ReinforcedConcreteSectionTest()
        {
            bool check = true;
            double width = 300;
            double height = 500;
            double concreteCover = 50;

            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(0, 0),
                new Point2d(width, 0),
                new Point2d(width, height),
                new Point2d(0, height)
            }));

            ShapeEx shapeEx = new ShapeEx(shape, ConcreteMaterialEN1992.C25_30);
            RebarSectionCircular rebar = new RebarSectionCircular(18, RebarMaterial.B450A);

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[]
            {
                new ReinforcedConcreteRebar(rebar, new Point2d(concreteCover, concreteCover)),
                new ReinforcedConcreteRebar(rebar, new Point2d(width - concreteCover, concreteCover)),
                new ReinforcedConcreteRebar(rebar, new Point2d(width - concreteCover, height - concreteCover)),
                new ReinforcedConcreteRebar(rebar, new Point2d(concreteCover, height - concreteCover))
            };

            ReinforcedConcreteSection s = new ReinforcedConcreteSection(shapeEx);
            s.AddRebars(rebars);

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ReinforcedConcreteSection oggettoDeserializzato = (ReinforcedConcreteSection)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Rebars.Count() != oggettoDeserializzato.Rebars.Count())
                        check = false;
                    for (int i = 0; i < s.Rebars.Count(); i++)
                        if (s.Rebars.ToArray()[i] != oggettoDeserializzato.Rebars.ToArray()[i])
                            check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Concrete_RectangularTest()
        {
            bool check = true;

            ConcreteSectionRectangular s = new ConcreteSectionRectangular(500, 600, ConcreteMaterialEN1992.C25_30, "section");
            s.AddRebars(new ReinforcedConcreteRebar[] {
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), Point2d.Origin),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(10, 10)),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(-10, -10))});

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ConcreteSectionRectangular oggettoDeserializzato = (ConcreteSectionRectangular)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Rebars.Count() != oggettoDeserializzato.Rebars.Count())
                        check = false;
                    for (int i = 0; i < s.Rebars.Count(); i++)
                        if (s.Rebars.ToArray()[i] != oggettoDeserializzato.Rebars.ToArray()[i])
                            check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Concrete_RebarCollectionTest1()
        {
            bool check = true;

            RebarCollection s = new RebarCollection();
            s.AddRange(new ReinforcedConcreteRebar[] {
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), Point2d.Origin, 50, 1, "a"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(10, 10), 60, 2, "b"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(-10, 10), 70, 3, "c"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(10, -10), 80, 4, "d"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(-10, -10), 90, 5, "e")});

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                RebarCollection oggettoDeserializzato = (RebarCollection)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Count != oggettoDeserializzato.Count)
                        check = false;
                    for(int i = 1; i <= s.Count; i++)
                        if(s.GetById(i) != oggettoDeserializzato.GetById(i))
                            check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Concrete_RebarCollectionTest2()
        {
            bool check = true;

            RebarCollection s = new RebarCollection();
            s.AddRange(new ReinforcedConcreteRebar[] {});

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                RebarCollection oggettoDeserializzato = (RebarCollection)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Count != oggettoDeserializzato.Count)
                        check = false;
                    for (int i = 1; i <= s.Count; i++)
                        if (s.GetById(i) != oggettoDeserializzato.GetById(i))
                            check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Concrete_RebarCollectionTest4()
        {
            bool check = true;

            RebarCollection s = new RebarCollection();
            s.AddRange(new ReinforcedConcreteRebar[] {
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), Point2d.Origin, 50, 1, "a"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), Point2d.Origin, 50, 2, "b"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(10, 10), 50, 3, "c"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(10, 10), 50, 4, "d"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(-10, 10), 50, 5, "e"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(-10, 10), 50, 6, "aa"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(10, -10), 50, 7, "as"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(10, -10), 50, 8, "ad"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(-10, -10), 50, 9, "af"),
                new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, RebarMaterial.B450C), new Point2d(-10, -10), 50, 10, "ae")});

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                RebarCollection oggettoDeserializzato = (RebarCollection)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Count != oggettoDeserializzato.Count)
                        check = false;
                    for (int i = 1; i <= s.Count; i++)
                        if (s.GetById(i) != oggettoDeserializzato.GetById(i))
                            check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        #endregion

        #region Generic Sections

        [TestMethod]
        public void Section_Steel_CircularTest()
        {
            bool check = true;

            SteelSectionCircular s = new SteelSectionCircular(10, SteelMaterial.S235, "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SteelSectionCircular oggettoDeserializzato = (SteelSectionCircular)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Material != oggettoDeserializzato.Material ||
                        s.Diameter != oggettoDeserializzato.Diameter ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Steel_CHSTest()
        {
            bool check = true;

            SteelSectionCHS s = new SteelSectionCHS(10, 2, SteelMaterial.S235, "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SteelSectionCHS oggettoDeserializzato = (SteelSectionCHS)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Material != oggettoDeserializzato.Material ||
                        s.Diameter != oggettoDeserializzato.Diameter ||
                        s.Thickness != oggettoDeserializzato.Thickness ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Steel_CTest()
        {
            bool check = true;

            SteelSectionC s = new SteelSectionC(200, 4, 100, 5, 100, 5, SteelMaterial.S235, "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SteelSectionC oggettoDeserializzato = (SteelSectionC)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Height != oggettoDeserializzato.Height ||
                        s.ThicknessWeb != oggettoDeserializzato.ThicknessWeb ||
                        s.LengthBottom != oggettoDeserializzato.LengthBottom ||
                        s.ThicknessBottom != oggettoDeserializzato.ThicknessBottom ||
                        s.LengthTop != oggettoDeserializzato.LengthTop ||
                        s.LengthTop != oggettoDeserializzato.LengthTop ||
                        s.ThicknessTop != oggettoDeserializzato.ThicknessTop ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Steel_HTest()
        {
            bool check = true;

            SteelSectionH s = new SteelSectionH(200, 4, 100, 5, 100, 5, SteelMaterial.S235, "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SteelSectionH oggettoDeserializzato = (SteelSectionH)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Height != oggettoDeserializzato.Height ||
                        s.ThicknessWeb != oggettoDeserializzato.ThicknessWeb ||
                        s.LenghtBottomFlange != oggettoDeserializzato.LenghtBottomFlange ||
                        s.LenghtTopFlange != oggettoDeserializzato.LenghtTopFlange ||
                        s.ThicknessTopFlange != oggettoDeserializzato.ThicknessTopFlange ||
                        s.ThicknessBottomFlange != oggettoDeserializzato.ThicknessBottomFlange ||
                        s.ThicknessWeb != oggettoDeserializzato.ThicknessWeb ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Steel_LTest()
        {
            bool check = true;

            SteelSectionL s = new SteelSectionL(200, 4, 100, 5, SteelMaterial.S235, "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SteelSectionL oggettoDeserializzato = (SteelSectionL)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.HorizontalLegLength != oggettoDeserializzato.HorizontalLegLength ||
                        s.HorizontalLegThickness != oggettoDeserializzato.HorizontalLegThickness ||
                        s.VerticalLegLength != oggettoDeserializzato.VerticalLegLength ||
                        s.VerticalLegThickness != oggettoDeserializzato.VerticalLegThickness ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Steel_RectangularTest()
        {
            bool check = true;

            SteelSectionRectangular s = new SteelSectionRectangular(200, 4, SteelMaterial.S235, "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SteelSectionRectangular oggettoDeserializzato = (SteelSectionRectangular)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Height != oggettoDeserializzato.Height ||
                        s.Width != oggettoDeserializzato.Width ||
                        s.Angle != oggettoDeserializzato.Angle ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Steel_RHSTest()
        {
            bool check = true;

            SteelSectionRHS s = new SteelSectionRHS(300, 200, 5, 5, 5, 5, SteelMaterial.S235, "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SteelSectionRHS oggettoDeserializzato = (SteelSectionRHS)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Height != oggettoDeserializzato.Height ||
                        s.Base != oggettoDeserializzato.Base ||
                        s.ThicknessTop != oggettoDeserializzato.ThicknessTop ||
                        s.ThicknessBottom != oggettoDeserializzato.ThicknessBottom ||
                        s.ThicknessWebLeft != oggettoDeserializzato.ThicknessWebLeft ||
                        s.ThicknessWebRight != oggettoDeserializzato.ThicknessWebRight ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Section_Steel_TTest()
        {
            bool check = true;

            SteelSectionT s = new SteelSectionT(300, 200, 5, 5, SteelMaterial.S235, "section");
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SteelSectionT oggettoDeserializzato = (SteelSectionT)casted;

                if (s.Equals(oggettoDeserializzato))
                {
                    if (s.Height != oggettoDeserializzato.Height ||
                        s.ThicknessWeb != oggettoDeserializzato.ThicknessWeb ||
                        s.ThicknessFlange != oggettoDeserializzato.ThicknessFlange ||
                        s.LenghtFlange != oggettoDeserializzato.LenghtFlange ||
                        s.Name != oggettoDeserializzato.Name)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {s} is serializable");
            else
                Console.WriteLine($"Warning: Class {s} is not serializable");

            Assert.IsTrue(check);
        }

        #endregion

        #endregion

        #region Materials

        [TestMethod]
        public void Material_Test()
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
				Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Material_ConcreteMaterialModelCode2010FRCTest()
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
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Material_ConcreteMaterialEN1992Test()
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
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Material_SteelMaterialTest()
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
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Material_RebarMaterialTest()
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
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        #endregion

        #region Result

        [TestMethod]
        public void Result_ResultBeamForcesTest()
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
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_ResultDisplacementTest()
        {
            bool check = true;

            ResultDisplacement m = new ResultDisplacement(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6, 1);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ResultDisplacement oggettoDeserializzato = (ResultDisplacement)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.D1 != oggettoDeserializzato.D1 ||
                        m.D2 != oggettoDeserializzato.D2 ||
                        m.D3 != oggettoDeserializzato.D3 ||
                        m.R1 != oggettoDeserializzato.R1 ||
                        m.R2 != oggettoDeserializzato.R2 ||
                        m.CoordinateSystem != oggettoDeserializzato.CoordinateSystem ||
                        m.Id != oggettoDeserializzato.Id ||
                        m.R3 != oggettoDeserializzato.R3)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_ResultPlateForcesTest()
        {
            bool check = true;

            ResultPlateForces m = new ResultPlateForces(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6, 7, 8, 1);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ResultPlateForces oggettoDeserializzato = (ResultPlateForces)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.Fxx != oggettoDeserializzato.Fxx ||
                        m.Fyy != oggettoDeserializzato.Fyy ||
                        m.Fzz != oggettoDeserializzato.Fzz ||
                        m.Fxy != oggettoDeserializzato.Fxy ||
                        m.Fxz != oggettoDeserializzato.Fxz ||
                        m.Fyz != oggettoDeserializzato.Fyz ||
                        m.Mxx != oggettoDeserializzato.Mxx ||
                        m.Myy != oggettoDeserializzato.Myy ||
                        m.Mzz != oggettoDeserializzato.Mzz ||
                        m.Mxy != oggettoDeserializzato.Mxy ||
                        m.Mxz != oggettoDeserializzato.Mxz ||
                        m.Myz != oggettoDeserializzato.Myz ||
                        m.CoordinateSystem != oggettoDeserializzato.CoordinateSystem ||
                        m.Id != oggettoDeserializzato.Id)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_ResultPlateStressTest()
        {
            bool check = true;

            ResultPlateStress m = new ResultPlateStress(CoordinateSystem.Global, 
                new ResultStress(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6, "a", 1),
                new ResultStress(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6, "b", 1),
                new ResultStress(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6, "c", 1),
                "Test", 1);

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ResultPlateStress oggettoDeserializzato = (ResultPlateStress)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.LowerFace != oggettoDeserializzato.LowerFace ||
                        m.MidFace != oggettoDeserializzato.MidFace ||
                        m.UpperFace != oggettoDeserializzato.UpperFace ||
                        m.CoordinateSystem != oggettoDeserializzato.CoordinateSystem ||
                        m.Id != oggettoDeserializzato.Id)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_ResultStressTest()
        {
            bool check = true;

            ResultStress m = new ResultStress(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6, "a", 1);

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ResultStress oggettoDeserializzato = (ResultStress)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.Sxx != oggettoDeserializzato.Sxx ||
                        m.Syy != oggettoDeserializzato.Syy ||
                        m.Szz != oggettoDeserializzato.Szz ||
                        m.Sxy != oggettoDeserializzato.Sxy ||
                        m.Sxz != oggettoDeserializzato.Sxz ||
                        m.Syz != oggettoDeserializzato.Syz ||
                        m.CoordinateSystem != oggettoDeserializzato.CoordinateSystem ||
                        m.Id != oggettoDeserializzato.Id)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_ResultLocationIdTest()
        {
            bool check = true;

            ResultLocationId m = new ResultLocationId(new ResultPlateForces[] {
                new ResultPlateForces(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6, 7, 8, 1)}, 1, "a");

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ResultLocationId oggettoDeserializzato = (ResultLocationId)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.ResultTypes != oggettoDeserializzato.ResultTypes ||
                        m.Id != oggettoDeserializzato.Id)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_ResultLocationStationTest()
        {
            bool check = true;

            ResultLocationStation m = new ResultLocationStation(new ResultBeamForces[] {
                new ResultBeamForces(1, 2, 3, 4, 5, 6, CoordinateSystem.Global, 1)}, 1, 2);

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ResultLocationStation oggettoDeserializzato = (ResultLocationStation)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.ResultTypes != oggettoDeserializzato.ResultTypes ||
                        m.DistanceFromStartPoint != oggettoDeserializzato.DistanceFromStartPoint ||
                        m.ElementLenght != oggettoDeserializzato.ElementLenght ||
                        m.Id != oggettoDeserializzato.Id)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_ResultLocationPointTest()
        {
            bool check = true;

            ResultLocationPoint m = new ResultLocationPoint(new ResultPlateForces[] {
                new ResultPlateForces(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6, 7, 1)}, Point2d.Origin, 1);

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                ResultLocationPoint oggettoDeserializzato = (ResultLocationPoint)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.Location != oggettoDeserializzato.Location ||
                        m.ResultTypes != oggettoDeserializzato.ResultTypes ||
                        m.Id != oggettoDeserializzato.Id)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_BeamResultTest()
        {
            bool check = true;

            BeamResult m = new BeamResult(new LoadCase("test", LoadCase.LoadCaseTypes.SuperImposedDeadLoad), new ResultLocationStation[] { }, 2);

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                BeamResult oggettoDeserializzato = (BeamResult)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.Case != oggettoDeserializzato.Case ||
                        m.Length != oggettoDeserializzato.Length ||
                        m.Name != oggettoDeserializzato.Name ||
                        m.ResultLocations != oggettoDeserializzato.ResultLocations ||
                        m.StageId != oggettoDeserializzato.StageId)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_BrickResultTest()
        {
            bool check = true;

            BrickResult m = new BrickResult(new LoadCase("test", LoadCase.LoadCaseTypes.SuperImposedDeadLoad),
                new ResultLocationPoint[] { new ResultLocationPoint(new ResultPlateForces[]{
                    new ResultPlateForces(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6, 7, 8, 22)}, Point2d.Origin, 5) }, 5);

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                BrickResult oggettoDeserializzato = (BrickResult)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.Case != oggettoDeserializzato.Case ||
                        m.Name != oggettoDeserializzato.Name ||
                        m.ResultLocations != oggettoDeserializzato.ResultLocations ||
                        m.StageId != oggettoDeserializzato.StageId)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_SectionResultTest()
        {
            bool check = true;

            SectionResult m = new SectionResult(new LoadCase("test", LoadCase.LoadCaseTypes.SuperImposedDeadLoad), new ResultLocationId[] { }, "");

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                SectionResult oggettoDeserializzato = (SectionResult)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.Case != oggettoDeserializzato.Case ||
                        m.Name != oggettoDeserializzato.Name ||
                        m.ResultLocations != oggettoDeserializzato.ResultLocations)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_NodeResultTest()
        {
            bool check = true;

            NodeResult m = new NodeResult(new LoadCase("test", LoadCase.LoadCaseTypes.SuperImposedDeadLoad), new ResultLocationId[] { }, 3);

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                NodeResult oggettoDeserializzato = (NodeResult)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.Case != oggettoDeserializzato.Case ||
                        m.Name != oggettoDeserializzato.Name ||
                        m.ResultLocations != oggettoDeserializzato.ResultLocations)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void Result_PlateResultTest()
        {
            bool check = true;

            PlateResult m = new PlateResult(new LoadCase("test", LoadCase.LoadCaseTypes.SuperImposedDeadLoad), new ResultLocationId[] { }, 3, "a");

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, m);
                ms.Position = 0;

                var casted = formatter.Deserialize(ms);
                PlateResult oggettoDeserializzato = (PlateResult)casted;

                if (m.Equals(oggettoDeserializzato))
                {
                    if (m.Case != oggettoDeserializzato.Case ||
                        m.Name != oggettoDeserializzato.Name ||
                        m.StageId != oggettoDeserializzato.StageId ||
                        m.ResultLocations != oggettoDeserializzato.ResultLocations)
                        check = false;
                }
                else
                {
                    check = false;
                }
            }

            if (check)
                Console.WriteLine($"Class {m} is serializable");
            else
                Console.WriteLine($"Warning: Class {m} is not serializable");

            Assert.IsTrue(check);
        }

        #endregion
    }
}