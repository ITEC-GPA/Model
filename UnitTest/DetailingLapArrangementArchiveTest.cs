using System.IO.Compression;
using System.Text.RegularExpressions;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Structure.Assignments;

namespace UnitTest;

/// <summary>
/// Arrangement of the laps in the detailing data of a concrete section (5.36.0.0): lapped percentage and number of layers, optional fields read by
/// the model checker for EN 1992-1-1 8.7.2(4). The fixtures hold the model of <see cref="Create"/> (two RC beams: «RC lap» in a lap zone with
/// cmin,dur 25 mm and the confirmations, «RC plain» without cmin,dur) saved by GPCModel 5.35.0.0 (origin/master 07fb7618) before the fields
/// existed: version 1 by ModelArchive.Save, version 4 by SaveDocument.
/// </summary>
[TestClass]
public class DetailingLapArrangementArchiveTest
{
    // Fingerprints computed by GPCModel 5.35.0.0 on the model of the fixtures.
    private const string LapData = "89ACCDEA67480107F6441B396A3161A3C6B057875567CFB1FEE28810FD9184BC";
    private const string LapSection = "4C9B76288CFABB9193D0EB2894B3E68AFCD370EA260B6B3E532F549A3D858198";
    private const string PlainData = "9C820DA99012E8982C1FD3598E9DD2F5D898938B4816A1E29F01A3723232A2E3";
    private const string PlainSection = "C5CB7F328D5ECF29AA2EDA4F0E33DCC78E515E15EF9155A16FAF4D2FB6D51A7E";

    /// <summary>The model of the fixtures, built as GPCModel 5.35.0.0 built it.</summary>
    private static GPC.Model.Models.Model Create(ConcreteDetailingData? lapData = null)
    {
        var model = new GPC.Model.Models.Model("Detailing data fixture");
        var points = new[] { new Point3d(0, 0, 0), new Point3d(0, 0, 3000), new Point3d(4000, 0, 3000) };
        for (int i = 0; i < 3; i++) model.NodesElements.Add(new NodeElement(points[i], id: 10 * (i + 1)));
        var bar = new RebarSectionCircular(16, SteelMaterialEN1992Data.B450C);
        ReinforcedConcreteSection Section(string name, ConcreteDetailingData data)
        {
            var section = new ReinforcedConcreteSection(new SectionRectangular(500, 300, name), ConcreteMaterialEN1992Data.C25_30);
            section.AddRebars(new[]{new ReinforcedConcreteRebar(bar,new Point2d(50,50)),new ReinforcedConcreteRebar(bar,new Point2d(250,50)),
                new ReinforcedConcreteRebar(bar,new Point2d(50,450)),new ReinforcedConcreteRebar(bar,new Point2d(250,450))});
            section.DetailingData = data; return section;
        }
        var lap = Section("RC lap", lapData ?? new ConcreteDetailingData(34, 25, 5, 300, 300, 300, 20, "drawings S-3", lapZone: true, compressionBarsRestrained: true, endZonesConfirmed: true));
        var plain = Section("RC plain", new ConcreteDetailingData(40, null, 10, 280, 300, 260, 16, "drawings S-4"));
        int id = 100;
        foreach (var (section, i, j) in new[] { (lap, 10, 20), (plain, 20, 30) })
        {
            var beam = new BeamElement(null, null, section, id: id); model.BeamElements.Add(beam); model.AddProperty(section); model.ConnectBeam(id, i, j);
            beam.Assignments.Sections.Add(new BeamSectionAssignment { Start = 0, End = 1, Section = section }); id++;
        }
        return model;
    }

    private static byte[] Fixture(string form)
    {
        using var file = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "detailing-legacy-5.35.0.0-" + form + ".xml.gz"));
        using var zip = new GZipStream(file, CompressionMode.Decompress); using var plain = new MemoryStream();
        zip.CopyTo(plain); return plain.ToArray();
    }
    private static byte[] Saved(GPC.Model.Models.Model model, string form)
    {
        using var stream = new MemoryStream();
        if (form == "v1") ModelArchive.Save(model, stream); else ModelArchive.SaveDocument(model, stream);
        return stream.ToArray();
    }
    private static string Fp(object value) => ModelValues.Fingerprint(new object[] { value });
    private static ReinforcedConcreteSection Section(GPC.Model.Models.Model model, int id) => model.BeamElements[id].Assignments.Sections[0].Section;
    // The geometry frames take new GUIDs at every construction and load, a model built anew also a new identity (unrelated to the detailing data).
    private static string WithoutGuids(byte[] xml, bool identity = false)
    {
        string text = Regex.Replace(System.Text.Encoding.UTF8.GetString(xml), "(<Guid [^>]*>)[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}(</Guid>)", "$1$2");
        return identity ? Regex.Replace(text, "(<Id>)[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}(</Id>)", "$1$2") : text;
    }

    [DataTestMethod] [DataRow("v1")] [DataRow("v4")]
    public void ArchivesWrittenBeforeTheLapFieldsLoadAndSaveUnchanged(string form)
    {
        var bytes = Fixture(form);
        StringAssert.Contains(System.Text.Encoding.UTF8.GetString(bytes), "<GpcModelArchive version=\"" + form.Substring(1) + "\">");
        var model = ModelArchive.Load(new MemoryStream(bytes));
        ConcreteDetailingData lap = Section(model, 100).DetailingData, plain = Section(model, 101).DetailingData;
        Assert.IsTrue(lap.LapZone); Assert.AreEqual(25, lap.MinimumDurabilityCover); Assert.IsTrue(lap.CompressionBarsRestrained && lap.EndZonesConfirmed);
        Assert.IsNull(lap.LappedPercent); Assert.IsNull(lap.LapLayers);
        Assert.IsFalse(plain.LapZone); Assert.IsNull(plain.MinimumDurabilityCover); Assert.AreEqual(260, plain.WebWidth); Assert.AreEqual("drawings S-4", plain.Source);
        Assert.IsNull(plain.LappedPercent); Assert.IsNull(plain.LapLayers);
        // The text of 5.35.0.0 is written again, both from the loaded model and from a model built now without the lap fields (saved before the
        // fingerprints below, which fill the cached outlines of the shapes, as the fixtures were).
        Assert.AreEqual(WithoutGuids(bytes), WithoutGuids(Saved(model, form)));
        var built = Saved(Create(), form);
        Assert.AreEqual(WithoutGuids(bytes, true), WithoutGuids(built, true));
        Assert.AreEqual(LapData, Fp(lap)); Assert.AreEqual(PlainData, Fp(plain));
        Assert.AreEqual(LapSection, Fp(Section(model, 100))); Assert.AreEqual(PlainSection, Fp(Section(model, 101)));
        StringAssert.DoesNotMatch(System.Text.Encoding.UTF8.GetString(built), new Regex("_lappedPercent|_lapLayers"));
    }

    [TestMethod]
    public void LapArrangementRoundTripsThroughEveryArchiveForm()
    {
        var data = new ConcreteDetailingData(34, 25, 5, 300, 300, 300, 20, "drawings S-3", true, true, true, 50, 2);
        Assert.AreEqual(50, data.LappedPercent); Assert.AreEqual(2, data.LapLayers);
        var model = Create(data);
        Assert.AreNotEqual(LapData, Fp(Section(model, 100).DetailingData), "the arrangement enters the fingerprint");
        Assert.AreEqual(PlainData, Fp(Section(model, 101).DetailingData));
        foreach (var form in new[] { "v1", "v4" })
        {
            var bytes = Saved(model, form); var text = System.Text.Encoding.UTF8.GetString(bytes);
            StringAssert.Contains(text, "_lappedPercent>50</", form); StringAssert.Contains(text, "_lapLayers>2</", form);
            // Only the section with the arrangement writes the members.
            Assert.AreEqual(1, Regex.Matches(text, "_lappedPercent>50<").Count, form); Assert.AreEqual(1, Regex.Matches(text, "_lapLayers>2<").Count, form);
            var copy = ModelArchive.Load(new MemoryStream(bytes));
            Assert.AreEqual(data, Section(copy, 100).DetailingData, form); Assert.AreEqual(Fp(data), Fp(Section(copy, 100).DetailingData), form);
            Assert.IsNull(Section(copy, 101).DetailingData.LappedPercent, form);
        }
        var value = ModelValues.Copy(model);
        Assert.AreEqual(data, Section(value, 100).DetailingData);
        // The fields are part of the value: equal data without them differ.
        Assert.AreNotEqual(new ConcreteDetailingData(34, 25, 5, 300, 300, 300, 20, "drawings S-3", true, true, true, null, null), data);
        Assert.AreEqual(new ConcreteDetailingData(34, 25, 5, 300, 300, 300, 20, "drawings S-3", true, true, true, null, null),
            new ConcreteDetailingData(34, 25, 5, 300, 300, 300, 20, "drawings S-3", lapZone: true, compressionBarsRestrained: true, endZonesConfirmed: true));
    }

    [TestMethod]
    public void LapArrangementIsValidated()
    {
        ConcreteDetailingData With(double? percent, int? layers) => new(34, 25, 5, 300, 300, 300, 20, "drawings", true, false, false, percent, layers);
        // Percentage in [0; 100], at least one layer, both or neither.
        Assert.AreEqual(0, With(0, 1).LappedPercent); Assert.AreEqual(100, With(100, 3).LappedPercent); Assert.AreEqual(3, With(100, 3).LapLayers);
        Assert.IsNull(With(null, null).LappedPercent);
        foreach (var percent in new[] { -1e-9, 100.000001, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => With(percent, 1), percent.ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => With(50, 0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => With(50, -2));
        Assert.ThrowsException<ArgumentException>(() => With(50, null));
        Assert.ThrowsException<ArgumentException>(() => With(null, 2));
    }
}
