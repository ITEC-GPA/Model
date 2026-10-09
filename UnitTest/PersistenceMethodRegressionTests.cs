using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using GPC.Model.Persistence;
using GPC.Model.Checking.Reports;

namespace UnitTest;

[TestClass]
public class PersistenceMethodRegressionTests
{
    [TestMethod]
    public void SaveDocument_LeavesDestinationOpenAndWritesAtCurrentPosition()
    {
        using var stream = new MemoryStream(); stream.WriteByte(42);
        ModelArchive.SaveDocument(new GPC.Model.Models.Model("document"), stream);
        Assert.IsTrue(stream.CanWrite); Assert.AreEqual(42, stream.ToArray()[0]);
        using var payload = new MemoryStream(stream.ToArray().Skip(1).ToArray());
        Assert.AreEqual("document", ModelArchive.Load(payload).Name);
    }
    [TestMethod]
    public void Load_LeavesCallerStreamOpen()
    {
        using var stream = new MemoryStream(); ModelArchive.SaveDocument(new GPC.Model.Models.Model("open"), stream); stream.Position = 0;
        ModelArchive.Load(stream); Assert.IsTrue(stream.CanRead); stream.Position = 0; Assert.IsTrue(stream.ReadByte() >= 0);
    }
    [TestMethod]
    public void Save_NullModelDoesNotModifyDestination()
    {
        using var stream = new MemoryStream(); stream.WriteByte(42);
        Assert.ThrowsException<ArgumentNullException>(() => ModelArchive.Save(null!, stream));
        CollectionAssert.AreEqual(new byte[] { 42 }, stream.ToArray()); Assert.AreEqual(1L, stream.Position);
    }
    [TestMethod]
    public void ReportSave_EmptyReportsRoundTripAndLeaveStreamsOpen()
    {
        using var stream = new MemoryStream(); CheckReportArchive.Save(Array.Empty<CheckReport>(), stream);
        Assert.IsTrue(stream.CanWrite); stream.Position = 0;
        Assert.AreEqual(0, CheckReportArchive.Load(stream).Count); Assert.IsTrue(stream.CanRead);
    }
    [TestMethod]
    public void ReportLoad_UnknownVersionIsRejected()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<GpcCheckReports version=\"99\"/>"));
        Assert.ThrowsException<SerializationException>(() => CheckReportArchive.Load(stream));
    }
    [TestMethod]
    public void ModelLoad_RejectsDtdBeforeReadingDomainValues()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<!DOCTYPE GpcModelArchive [<!ENTITY name 'internal'>]><GpcModelArchive version=\"4\">&name;</GpcModelArchive>"));
        Assert.ThrowsException<XmlException>(() => ModelArchive.Load(stream));
    }
}
