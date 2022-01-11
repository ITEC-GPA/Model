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
                    Console.WriteLine($"Class {objToTest.ToString().Replace("GPC.Geometry", "")} is serializable");
                }
                else
                {
                    Console.WriteLine($"Warning: Class {objToTest.ToString().Replace("GPC.Geometry", "")} is not serializable");
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
        public void SerializableClassStandardCopSuos2011Test()
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
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassStandardEN16612Test()
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
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassStandardEN1990Test()
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
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassStandardEN1993p11Test()
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
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassStandardUNIEN1993p11Test()
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
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassStandardModelCode2010Test()
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
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassStandardCNR204Test()
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
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassStandardNTC2018ConcreteTest()
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
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassStandardEN1992p11Test()
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
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }

        [TestMethod]
        public void SerializableClassStandardUNIEn1992p11Test()
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
                Console.WriteLine($"Class {s.ToString().Replace("GPC.Geometry.", "")} is serializable");
            else
                Console.WriteLine($"Warning: Class {s.ToString().Replace("GPC.Geometry", "")} is not serializable");

            Assert.IsTrue(check);
        }
    }
}