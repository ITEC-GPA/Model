using GPC.Model.Actions.Climate;
using GPC.Model.Persistence;

namespace UnitTest;

[TestClass]
public sealed class ClimateContractsTests
{
    [TestMethod]
    public void ClimateEnumVocabularyRoundTripsEveryDeclaredValueWithoutBinaryFormatter()
    {
        var types=new[]{typeof(GPC.Model.Actions.Traffic.TransverseDistributionMethod),typeof(ClimateStandard),typeof(SnowZone),typeof(SnowExposure),typeof(SnowRoof),typeof(WindRoof),typeof(InternalPressure),typeof(BuildingThermalMethod),typeof(ThermalMaterial),typeof(BridgeDeck),typeof(BridgeSurfacing)};
        types=types.Concat(typeof(GPC.Model.Actions.General.BuildingLoadsInput).Assembly.GetTypes().Where(t=>t.IsEnum&&t.Namespace=="GPC.Model.Actions.General")).ToArray();
        var source=types.SelectMany(t=>Enum.GetValues(t).Cast<object>()).ToArray();
        var copy=ModelArchive.CopyValue(source);
        CollectionAssert.AreEqual(source,copy);
        foreach(var type in types)Assert.IsTrue(ArchiveContractRegistry.SupportedTypes.Contains(type));
        Assert.AreEqual(ModelArchive.Fingerprint(source),ModelArchive.Fingerprint(copy));
    }
}
