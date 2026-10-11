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
    [Ignore("Difetto segnalato: SectionColdFormed scrive la linea media come Point2d[], che non puo' entrare nel vocabolario degli archivi (stesso nome di contratto di Polygon2d, ArrayOfPoint2d); un modello con sezioni sagomate a freddo non si salva.")]
    public void ColdFormedSectionsCanBeArchived()
    {
        var section = SectionColdFormed.LippedChannel(200, 75, 20, 2, 3, "C");
        var copy = ModelArchive.CopyValue(section);
        Assert.AreEqual(section.Area, copy.Area, 1e-9 * section.Area);
        Assert.AreEqual(section.MiddleLine.Count, copy.MiddleLine.Count);
    }

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
