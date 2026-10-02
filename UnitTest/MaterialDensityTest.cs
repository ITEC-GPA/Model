using GPC.Model;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Geotechnics;
using GPC.Model.Loads;
using GPC.Model.Materials;
using GPC.Utilities.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Runtime.Serialization;

namespace ModelObjectTest
{
    /// <summary>
    /// The density of the materials in t/mm³ (mass unit of the default units over mm³): defaults, unit weight, constructors and the files written
    /// before the version 5 of the material (density in g/mm³)
    /// </summary>
    [TestClass]
    public class MaterialDensityTest
    {
        [TestMethod]
        public void DefaultsAreInTonnesPerCubicMillimetre()
        {
            Assert.AreEqual(7.85e-9, Material.SteelDensity);
            Assert.AreEqual(2.5e-9, Material.ConcreteDensity);
            Assert.AreEqual(2.7e-9, Material.AluminiumDensity);
            // 7850 kg/m³ = 7850e-3 t / 1e9 mm³
            Assert.AreEqual(7850.0, Material.SteelDensity.ConvertDensityFromDefault(GPC.Model.Units.SI), 1e-9);
            Assert.AreEqual(Material.SteelDensity, 7850.0.ConvertDensityToDefault(GPC.Model.Units.SI), 1e-24);
            Assert.AreEqual(7.85, Material.SteelDensity.ConvertDensityFromDefault(GPC.Model.Units.Knm), 1e-12);

            foreach (Material steel in new Material[] { SteelMaterialEN1993Data.S355, SteelMaterialEN1992Data.B450C, new SteelMaterial("s", 210000, 355, 510),
                new SteelMaterialEN1993("s", 210000, 355, 510), new SteelMaterialDM1996("s", 206000, 375, 540), new BoltMaterialEN1993("b", 210000, 640, 800),
                new SteelMaterialACI318("s", 200000, 420, 620), new SteelMaterialAISC360("s", 200000, 345, 450), new FRP("f") })
                Assert.AreEqual(Material.SteelDensity, steel.Density, steel.Name);
            foreach (Material concrete in new Material[] { ConcreteMaterialEN1992Data.C25_30, ConcreteMaterialModelCode2010Data.C30_37,
                ConcreteMaterialACI318Data.Fc4000, new ConcreteMaterialEN1992("c", 25, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle) })
                Assert.AreEqual(Material.ConcreteDensity, concrete.Density, concrete.Name);
            foreach (Material aluminium in new Material[] { GPC.Model.Data.Aluminium.AluminiumMaterialEN1999Data.Aluminum6061_T6, new AluminiumMaterial("a", AluminiumMaterial.AluminiumTypes.Structural) })
                Assert.AreEqual(Material.AluminiumDensity, aluminium.Density, aluminium.Name);
        }

        [TestMethod]
        public void UnitWeightIsDensityByGravity()
        {
            SteelMaterial steel = SteelMaterialEN1993Data.S355;
            // t/mm³ · mm/s² = N/mm³: the gravity of the model (9806.65 mm/s²) by default, 9810 for the geotechnical unit weights
            Assert.AreEqual(7.85e-9 * ModelGravityLoad.GRAVITYACCELERATION, steel.GetUnitWeight(), 1e-20);
            Assert.AreEqual(76.982, steel.GetUnitWeight() / SoilUnits.KiloNewtonPerCubicMetre, 5e-4);
            Assert.AreEqual(7850 * 9.81 * 1e-9, steel.GetUnitWeight(SoilUnits.Gravity), 1e-18);
            Assert.AreEqual(77.0085, steel.GetUnitWeight(SoilUnits.Gravity) / SoilUnits.KiloNewtonPerCubicMetre, 1e-9);
            Assert.AreEqual(24.525, ConcreteMaterialEN1992Data.C25_30.GetUnitWeight(SoilUnits.Gravity) / SoilUnits.KiloNewtonPerCubicMetre, 1e-9);
            // the water of the geotechnics is the density 1 t/m³ by the same gravity
            Assert.AreEqual(SoilUnits.WaterUnitWeight, 1e-9 * SoilUnits.Gravity, 1e-20);
            Assert.AreEqual(0, new SteelMaterial("s", 210000, 355, 510, density: 0).GetUnitWeight());
        }

        [TestMethod]
        public void ConstructorsFromTheTablesKeepDensityAndThermalExpansion()
        {
            var compression = new StressStrainTable(new double[] { 0, -355 }, new double[] { 0, -0.01 });
            var tension = new StressStrainTable(new double[] { 0, 355 }, new double[] { 0, 0.01 });
            // the table constructors passed the density as thermal expansion and the thermal expansion as density
            var steel = new SteelMaterial("s", 210000, 210000, -0.0017, -0.01, 0.0017, 0.01, -355, -355, 355, 355, compression, tension);
            Assert.AreEqual(Material.SteelDensity, steel.Density);
            Assert.AreEqual(12e-6, steel.AlfaThermalExpansion);
            steel = new SteelMaterial("s", 210000, 210000, -0.0017, -0.01, 0.0017, 0.01, -355, -355, 355, 355, compression, tension, density: 8e-9, alfaThermalExpansion: 1.1e-5);
            Assert.AreEqual(8e-9, steel.Density);
            Assert.AreEqual(1.1e-5, steel.AlfaThermalExpansion);
            var en1993 = new SteelMaterialEN1993("s", 210000, 210000, -0.0017, -0.01, 0.0017, 0.01, -355, -355, 355, 355, compression, tension);
            Assert.AreEqual(Material.SteelDensity, en1993.Density);
            Assert.AreEqual(1.2e-5, en1993.AlfaThermalExpansion);

            var frp = new FRP("f", 210000, 210000, -0.0017, -0.01, 0.0017, 0.01, -355, -355, 355, 355, compression, tension);
            Assert.AreEqual(Material.SteelDensity, frp.Density);
            Assert.AreEqual(12e-6, frp.AlfaThermalExpansion);

            var aluminium = new AluminiumMaterial("a", 70000, 70000, -0.003, -0.04, 0.003, 0.04, -240, -260, 240, 260, compression, tension);
            Assert.AreEqual(Material.AluminiumDensity, aluminium.Density);
            Assert.AreEqual(23e-6, aluminium.AlfaThermalExpansion);
            Assert.AreEqual(0.3, aluminium.Ni);
            // EN 1999: Poisson's ratio and maximum thickness were swapped (Poisson 5 with the defaults: the constructor threw)
            var en1999 = new AluminiumMaterialEN1999("a", 70000, 70000, -0.003, -0.04, 0.003, 0.04, -240, -260, 240, 260, compression, tension);
            Assert.AreEqual(0.3, en1999.Ni);
            Assert.AreEqual(5, en1999.ThicknessMax);
            Assert.AreEqual(Material.AluminiumDensity, en1999.Density);
            Assert.AreEqual(23e-6, en1999.AlfaThermalExpansion);
        }

        [TestMethod]
        public void SerializationKeepsTheDensity()
        {
            SteelMaterial steel = SteelMaterialEN1993Data.S355;
            var copy = (SteelMaterial)Serialization.DeserializeFromBytes(Serialization.SerializeToBytes(steel));
            Assert.AreEqual(Material.SteelDensity, copy.Density);
            Assert.AreEqual(steel.AlfaThermalExpansion, copy.AlfaThermalExpansion);
            // values not converted in the version 5, also the impossible ones
            var odd = new SteelMaterial("s", 210000, 355, 510, density: 0.00785);
            Assert.AreEqual(0.00785, ((SteelMaterial)Serialization.DeserializeFromBytes(Serialization.SerializeToBytes(odd))).Density);
        }

        [TestMethod]
        public void FilesBeforeTheVersion5AreConvertedFromGramsPerCubicMillimetre()
        {
            // the old defaults (g/mm³) become t/mm³
            Assert.AreEqual(7.85e-9, Legacy(new SteelMaterial("s", 210000, 355, 510), 0.00785, 4).Density, 1e-22);
            Assert.AreEqual(2.5e-9, Legacy(ConcreteMaterialEN1992Data.C25_30, 0.0025, 4).Density, 1e-22);
            Assert.AreEqual(2.7e-9, Legacy(new AluminiumMaterial("a", AluminiumMaterial.AluminiumTypes.Structural), 0.0027, 3).Density, 1e-22);
            // a value of a material without version (1) is converted as well
            Assert.AreEqual(7.85e-9, Legacy(new SteelMaterial("s", 210000, 355, 510), 0.00785, null).Density, 1e-22);
            // densities already in t/mm³ (e.g. the glass, documented in T/mm³) and zero are kept
            Assert.AreEqual(2.5e-9, Legacy(new SteelMaterial("s", 210000, 355, 510), 2.5e-9, 4).Density);
            Assert.AreEqual(1e-6, Legacy(new SteelMaterial("s", 210000, 355, 510), 1e-6, 4).Density);
            Assert.AreEqual(0, Legacy(new SteelMaterial("s", 210000, 355, 510), 0, 4).Density);
            // the version 5 is never converted
            Assert.AreEqual(0.00785, Legacy(new SteelMaterial("s", 210000, 355, 510), 0.00785, 5).Density);
        }

        /// <summary>
        /// The material read from its serialization data with another version and density (the version is removed when null)
        /// </summary>
        private static T Legacy<T>(T material, double density, double? version) where T : Material
        {
            var context = new StreamingContext(StreamingContextStates.All);
            var info = new SerializationInfo(typeof(T), new FormatterConverter());
            material.GetObjectData(info, context);
            var old = new SerializationInfo(typeof(T), new FormatterConverter());
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "MaterialVersion")
                {
                    if (version.HasValue)
                        old.AddValue(entry.Name, version.Value);
                    else // the version 1 had one elastic modulus
                        old.AddValue("ElasticModulus", material.ElasticModulusCompression);
                }
                else
                    old.AddValue(entry.Name, entry.Name == "Density" ? density : entry.Value, entry.ObjectType);
            }
            return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object[] { old, context }, null)!;
        }
    }
}
