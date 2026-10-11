using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.Sections;

namespace UnitTest;

/// <summary>
/// Architecture review R10: the archives are compatible backwards only. Data written by a newer GPCModel, with a serialization version
/// newer than the one a class writes, are refused instead of read with the older layout (ANTHEA register S4-1: generic sections of
/// version 5 read with NaN properties by a Model that knew version 4).
/// </summary>
[TestClass]
public class ForwardCompatibilityTest
{
    [TestMethod]
    public void EveryLayerRefusesItsNewerVersionAndReadsTheCurrentOne()
    {
        var section = new SectionH(300, 11, 300, 19, 300, 19, "HE 300 B", 27);
        foreach (var entry in new[] { "SectionHVersion", "ThinWallSectionVersion", "SectionVersion" }) AssertNewerRefused(section, entry);
        AssertNewerRefused(new SectionEllipse(400, 300, 20, "E"), "SectionEllipseVersion");
        var concrete = ConcreteMaterialEN1992Data.C30_37;
        foreach (var entry in new[] { "ConcreteMaterialEuropeanCommonVersion", "ConcreteMaterialVersion", "MaterialVersion" }) AssertNewerRefused(concrete, entry);
        var node = new NodeElement(new Point3d(1, 2, 3), id: 7);
        foreach (var entry in new[] { "NodeElementVersion", "ElementVersion" }) AssertNewerRefused(node, entry);
    }

    [TestMethod]
    public void AnArchiveOfANewerModelIsRefusedWithTheReason()
    {
        var model = PostProcessingTest.Mixed();
        using var document = new MemoryStream(); ModelArchive.SaveDocument(model, document);
        string xml = Encoding.UTF8.GetString(document.ToArray());
        var version = new Regex("(<ElementVersion[^>]*>)1(</ElementVersion>)");
        Assert.IsTrue(version.IsMatch(xml), "The archive writes the version of the elements.");
        using var newer = new MemoryStream(Encoding.UTF8.GetBytes(version.Replace(xml, "${1}2${2}", 1)));
        var error = Assert.ThrowsException<SerializationException>(() => ModelArchive.Load(newer));
        StringAssert.Contains(Messages(error), "NewerSerializationVersion");
        document.Position = 0; Assert.AreEqual(model.NodesElements.Count, ModelArchive.Load(document).NodesElements.Count);
    }

    private static void AssertNewerRefused(ISerializable value, string entry)
    {
        var type = value.GetType();
        var info = new SerializationInfo(type, new FormatterConverter()); value.GetObjectData(info, new StreamingContext());
        var newer = new SerializationInfo(type, new FormatterConverter()); bool found = false;
        foreach (SerializationEntry e in info)
        {
            object v = e.Value;
            if (e.Name == entry) { found = true; v = v is int i ? i + 1 : Convert.ToDouble(v) + 1; }
            newer.AddValue(e.Name, v, e.ObjectType);
        }
        Assert.IsTrue(found, type.Name + " writes " + entry);
        var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null)!;
        var error = Assert.ThrowsException<TargetInvocationException>(() => constructor.Invoke(new object[] { newer, new StreamingContext() }));
        Assert.IsInstanceOfType(error.InnerException, typeof(SerializationException), type.Name + " " + entry);
        StringAssert.Contains(error.InnerException!.Message, "NewerSerializationVersion: " + type.FullName + " " + entry);
        Assert.IsNotNull(constructor.Invoke(new object[] { info, new StreamingContext() }));
    }

    private static string Messages(Exception error)
    {
        var text = new StringBuilder();
        for (var e = error; e != null; e = e.InnerException) text.AppendLine(e.Message);
        return text.ToString();
    }
}
