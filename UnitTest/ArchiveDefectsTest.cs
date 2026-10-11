using GPC.Geometry;
using GPC.Model.Collections;
using GPC.Model.Data.Steel;
using GPC.Model.Persistence;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;

namespace UnitTest;

/// <summary>
/// Defects of the archives found while the tests left BinaryFormatter (architecture review R10, 11/10/2026). Reported to the user and
/// not corrected: the tests state the expected behaviour and are ignored until the correction is decided.
/// </summary>
[TestClass]
public class ArchiveDefectsTest
{
    [TestMethod]
    [Ignore("Difetto segnalato: il contratto dati scrive UniqueIdCollection come dizionario e MaxId si perde; dopo il salvataggio un id cancellato puo' essere riusato.")]
    public void IdCollectionsKeepTheirLargestIdThroughTheArchive()
    {
        var rebars = new UniqueIdCollection<ReinforcedConcreteRebar>();
        rebars.Add(new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, SteelMaterialEN1992Data.B450C), Point2d.Origin, 50, 1, "a"));
        rebars.Add(new ReinforcedConcreteRebar(new RebarSectionCircular("", 10, SteelMaterialEN1992Data.B450C), new Point2d(10, 0), 50, 7, "b"));
        rebars.Remove(7);
        Assert.AreEqual(7, ModelArchive.CopyValue(rebars).MaxId);
    }
}
