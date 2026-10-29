using System.Globalization;
using System.Text.Json;
using GPC.Model.Standards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ConcreteStandardCatalogTests
{
    // One accepted change since the port (ANTHEA 1.16.0, R6, user decisions of 28/10/2026 «Fonti secondarie» and «Approvati se spiegati»):
    // DIN EN 1992-1-1 ServiceabilityStressPrestressSteelCoefficientForCharacteristicCombination 0.75 -> 0.65 (k5 of DIN NA NDP 7.2(5)).
    [TestMethod]
    public void ProfilesMatchImmutablePrePortCoefficients()
    {
        using var data=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Fixtures","concrete-profiles-before.json")));
        int count=0;
        foreach(var row in data.RootElement.EnumerateArray())
        {
            string name=row.GetProperty("input").GetProperty("name").GetString()!;
            var kind=ConcreteStandardCatalog.Parse(name); var standard=ConcreteStandardCatalog.Create(kind);
            Assert.AreEqual(name,ConcreteStandardCatalog.Name(kind));
            foreach(var coefficient in row.GetProperty("expected").GetProperty("result").EnumerateObject())
                Assert.AreEqual(coefficient.Value.GetString(),((double)typeof(StandardModelCode2010).GetProperty(coefficient.Name)!.GetValue(standard)!).ToString("G12",CultureInfo.InvariantCulture),name+"/"+coefficient.Name);
            var other=ConcreteStandardCatalog.Create(kind);standard.GammaC=99;Assert.AreNotEqual(99d,other.GammaC); count++;
        }
        Assert.AreEqual(9,count);
    }
    [TestMethod]
    public void MissingOrUnknownStandardsNeverFallBack()
    {
        foreach(string name in new[]{"", "NTC", "EN 1992-1-1:2023", "unknown"})
            Assert.ThrowsException<ArgumentException>(()=>ConcreteStandardCatalog.Parse(name));
        Assert.ThrowsException<ArgumentException>(()=>ConcreteStandardCatalog.Create((ConcreteStandardKind)999));
        Assert.ThrowsException<ArgumentException>(()=>ConcreteStandardCatalog.Name((ConcreteStandardKind)999));
    }
}
