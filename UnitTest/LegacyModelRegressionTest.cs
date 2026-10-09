using GPC.Geometry;
using GPC.Model.Collections;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Persistence;
using GPC.Model.Restraints;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using GPC.Model.Structure.Assignments;

#pragma warning disable SYSLIB0011
namespace UnitTest
{
    [TestClass]
    public class LegacyModelRegressionTest
    {
        [TestMethod]
        public void TrustedLegacyRoundTripRestoresCollectionItemsAndSharedNodes()
        {
            var m = PostProcessingTest.Mixed(); using var stream = new MemoryStream(); var formatter = new BinaryFormatter();
            formatter.Serialize(stream, m); stream.Position = 0; var copy = (GPC.Model.Models.Model)formatter.Deserialize(stream);
            Assert.AreEqual(4, copy.NodesElements.Count); Assert.AreSame(copy.NodesElements[10], copy.BeamElements[10].NodeI);
        }
        [TestMethod]
        public void LegacyNodeWriterKeyMigratesAndMissingElementCollectionsAreInitialized()
        {
            var node = new NodeElement(new Point3d(1, 2, 3), id: 80); var info = Info(node);
            var legacy = new SerializationInfo(typeof(NodeElement), new FormatterConverter());
            foreach (SerializationEntry e in info)
                if (e.Name != "Attributes" && e.Name != "Loads" && e.Name != "Results") legacy.AddValue(e.Name == "NodeElementVersion" ? "BeamVersion" : e.Name, e.Value);
            var restored = new NodeReader(legacy); Assert.AreEqual(80, restored.Id); Assert.AreEqual(3, restored.Position.Z, 1e-12);
            Assert.AreEqual(0, restored.Loads.Count); Assert.AreEqual(0, restored.Results.Count); Assert.AreEqual(0, restored.Attributes.Count);
        }
        private sealed class NodeReader : NodeElement { public NodeReader(SerializationInfo i) : base(i, new StreamingContext()) { } }
        [TestMethod]
        public void LegacyDisplacementStationKeyMigratesAndRejectsNonFinite()
        {
            var result = new StationResultDisplacement(new LoadCaseBase("A"), new ResultDisplacement(1, 2, 3, 4, 5, 6), .3);
            var info = Info(result); var legacy = new SerializationInfo(typeof(StationResultDisplacement), new FormatterConverter());
            foreach (SerializationEntry e in info) legacy.AddValue(e.Name == "ParametricDistance" ? "DistanceFromStartPoint" : e.Name, e.Value);
            Assert.AreEqual(.3, new DisplacementReader(legacy).ParametricDistance, 1e-12);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => result.ParametricDistance = double.NaN);
        }
        private sealed class DisplacementReader : StationResultDisplacement { public DisplacementReader(SerializationInfo i) : base(i, new StreamingContext()) { } }
        [TestMethod]
        public void RestrainStiffnessDoesNotBecomeImposedDisplacement()
        {
            var node = new NodeElement(Point3d.Origin); var restraint = new NodeRestrain(node, CoordinateSystem.Global);
            restraint.AddStiffness(GeometryRestrain.DOF.DX, 123); restraint.AddImposedDisplacement(GeometryRestrain.DOF.DX, 2);
            Assert.AreEqual(123, restraint.GetStiffnesses()[GeometryRestrain.DOF.DX], 1e-12);
            Assert.AreEqual(2, restraint.GetImposedDisplacement()[GeometryRestrain.DOF.DX], 1e-12);
            Assert.IsTrue(restraint.Restrains[0].Equals((object)new DofRestrain(GeometryRestrain.DOF.DX, false, 2, 123)));
        }
        [TestMethod]
        public void FutureArchiveVersionsAndDtdAreRejected()
        {
            using var future = new MemoryStream(Encoding.UTF8.GetBytes("<GpcModelArchive version=\"999\"/>"));
            Assert.ThrowsException<SerializationException>(() => ModelArchive.Load(future));
            using var dtd = new MemoryStream(Encoding.UTF8.GetBytes("<!DOCTYPE GpcModelArchive [<!ENTITY x 'test'>]><GpcModelArchive version=\"1\">&x;</GpcModelArchive>"));
            Assert.ThrowsException<System.Xml.XmlException>(() => ModelArchive.Load(dtd));
        }
        [TestMethod]
        public void NegativeAndAsymmetricSpringDataArePreservedButDiagnosed()
        {
            var matrix = new double[36]; matrix[0] = -10; matrix[1] = 3;
            var spring = new SpringMatrix(matrix, CoordinateSystem.Global);
            Assert.AreEqual(-10, spring[0, 0], 1e-12); Assert.IsTrue(spring.Diagnose().Any(d => d.Code == "NonSymmetricSpring"));
            Assert.ThrowsException<NotSupportedException>(() => spring.Apply(new double[6]));
            Assert.ThrowsException<ArgumentException>(() => new SpringMatrix(new double[6], CoordinateSystem.Global));
            matrix[2] = double.NaN; Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SpringMatrix(matrix, CoordinateSystem.Global));
        }
        private static SerializationInfo Info(ISerializable value)
        {
            var info = new SerializationInfo(value.GetType(), new FormatterConverter()); value.GetObjectData(info, new StreamingContext()); return info;
        }
    }
}
