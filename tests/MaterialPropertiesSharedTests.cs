using GPC.Model.Materials;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text.Json;
namespace ModelObjectTest;
[TestClass]
public sealed class MaterialPropertiesSharedTests
{
    [TestMethod]
    public void SharedModuliPreserveBothCapturedUnitEvaluationOrdersBitForBit()
    {
        byte[] bytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures/material-modulus-before.json"));
        Assert.AreEqual("29807390FFFF7D62CD0C1A91B1046B3E9EE0863A10D212D5EF98A2BF03DCD6FC", Convert.ToHexString(SHA256.HashData(bytes)));
        using var capture = JsonDocument.Parse(bytes);
        Assert.AreEqual(capture.RootElement.GetProperty("densityBits").GetInt64(), BitConverter.DoubleToInt64Bits(Material.SteelDensity));
        int count = 0;
        foreach (var row in capture.RootElement.GetProperty("rows").EnumerateArray())
        {
            double fck = row.GetProperty("fck").GetDouble();
            var material = new ConcreteMaterialEN1992("shared", fck, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
            Assert.AreEqual(row.GetProperty("modelBits").GetInt64(), BitConverter.DoubleToInt64Bits(material.Ecm));
            Assert.AreEqual(row.GetProperty("bridgeBits").GetInt64(), BitConverter.DoubleToInt64Bits(EuropeanConcreteElasticity.SecantModulusMpa(fck + 8)));
            Assert.AreEqual(EuropeanConcreteElasticity.SecantModulusMpa(fck + 8), EuropeanConcreteElasticity.SecantModulusMpa(-fck - 8));
            count++;
        }
        Assert.AreEqual(141, count);
    }
    [TestMethod]
    public void ReferenceStrengthAndDensityUseDeclaredUnits()
    {
        Assert.AreEqual(22d, EuropeanConcreteElasticity.SecantModulusGpa(10));
        Assert.AreEqual(22000d, EuropeanConcreteElasticity.SecantModulusMpa(10));
        Assert.AreEqual(7850d, Material.SteelDensityKgPerCubicMetre);
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(7.85), BitConverter.DoubleToInt64Bits(Material.SteelDensityTonnesPerCubicMetre));
    }
}
