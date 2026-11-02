using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Core.Identity;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Structure.Assignments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    // Compiled in the current .NET and .NET Framework runners: the production domain is netstandard2.0.
    [TestClass]
    public class MigrationCopyContractsTest
    {
        [TestMethod]
        public void StandaloneGraphKeepsAliasesIdentityAndRemovedLoadIdsWithoutSharingMutableData()
        {
            var point = new Point3d(1, 2, 3);
            var a = new NodeElement(point, id: 11, name: "A");
            var b = new NodeElement(point, id: 12, name: "B");
            var c = new NodeElement(new Point3d(2, 5, 8), id: 13);
            a.Source = new SourceIdentity("GH", "r1", EntityFamily.Node, "node-a");
            a.Assignments.Active = false;
            var loadCase = new LoadCaseBase("G");
            var removed = new PointLoad(1, 2, 3, 4, 5, 6, point, loadCase);
            a.Loads.Add(removed); int removedId = removed.Id; a.Loads.Remove(removedId);
            var shell = new AreaElement(null, null, id: 14);
            shell.ConnectNodes(a, b, c);
            shell.Assignments.Offset = -17;
            shell.Assignments.LayerAxes = a.CoordinateSystem;
            var original = new object[] { a, b, c, shell, point, loadCase };
            var copied = ModelValues.CopyValue(original);
            var nodeCopy = (NodeElement)copied[0]; var shellCopy = (AreaElement)copied[3];
            Assert.AreEqual(a.Guid, nodeCopy.Guid);
            Assert.AreEqual(a.Source, nodeCopy.Source);
            Assert.AreSame(nodeCopy, shellCopy.Nodes[0]);
            Assert.AreSame(nodeCopy.Position, ((NodeElement)copied[1]).Position);
            Assert.AreSame(nodeCopy.Position, copied[4]);
            Assert.AreSame(nodeCopy.CoordinateSystem, shellCopy.Assignments.LayerAxes);
            Assert.AreNotSame(a.Assignments, nodeCopy.Assignments);
            Assert.AreEqual(false, nodeCopy.Assignments.Active);
            Assert.AreEqual(-17.0, shellCopy.Assignments.Offset);
            var added = new PointLoad(7, 8, 9, 0, 0, 0, nodeCopy.Position, (LoadCaseBase)copied[5]);
            nodeCopy.Loads.Add(added);
            Assert.AreEqual(removedId + 1, added.Id);
            nodeCopy.Position.X = 99;
            nodeCopy.Assignments.Active = true;
            Assert.AreEqual(1.0, point.X);
            Assert.AreEqual(false, a.Assignments.Active);
            Assert.AreEqual(0, a.Loads.Count);
        }

        [TestMethod]
        public void GeometricLeavesKeepEveryDoubleBitAndAreDetached()
        {
            double negativeZero = BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000UL));
            double payloadNaN = BitConverter.Int64BitsToDouble(0x7ff8000000000042L);
            var point = new Point3d(negativeZero, payloadNaN, double.PositiveInfinity);
            var vector = new Vector3d(double.Epsilon, -double.Epsilon, double.NegativeInfinity);
            var result = ModelValues.CopyValue(new object[] { point, vector, point });
            var p = (Point3d)result[0]; var v = (Vector3d)result[1];
            Assert.AreSame(p, result[2]); Assert.AreNotSame(point, p); Assert.AreNotSame(vector, v);
            CollectionAssert.AreEqual(new[] { point.X, point.Y, point.Z }.Select(BitConverter.DoubleToInt64Bits).ToArray(),
                new[] { p.X, p.Y, p.Z }.Select(BitConverter.DoubleToInt64Bits).ToArray());
            CollectionAssert.AreEqual(new[] { vector.X, vector.Y, vector.Z }.Select(BitConverter.DoubleToInt64Bits).ToArray(),
                new[] { v.X, v.Y, v.Z }.Select(BitConverter.DoubleToInt64Bits).ToArray());
        }

        [Serializable]
        private sealed class UnknownNode : NodeElement
        {
            internal UnknownNode() : base(new Point3d(1, 2, 3)) { }
        }

        [TestMethod]
        public void UnknownDerivedDomainObjectsStillFailInsteadOfLosingFields()
        {
            Assert.ThrowsException<SerializationException>(() => ModelValues.CopyValue(new UnknownNode()));
            Assert.ThrowsException<SerializationException>(() => ModelValues.CopyValue(new object[] { new UnknownNode() }));
        }

        [TestMethod]
        public void MatrixCopiesRetainStorageAndDataAliasesWithoutSharingTheInputBuffer()
        {
            var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };
            var matrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(2, 3, data);
            var storage = (MathNet.Numerics.LinearAlgebra.Storage.DenseColumnMajorMatrixStorage<double>)matrix.Storage;
            var second = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(storage);
            var copy = ModelValues.CopyValue(new object[] { matrix, storage, data, second });
            var m = (MathNet.Numerics.LinearAlgebra.Double.DenseMatrix)copy[0];
            var s = (MathNet.Numerics.LinearAlgebra.Storage.DenseColumnMajorMatrixStorage<double>)copy[1];
            Assert.AreSame(s, m.Storage);
            Assert.AreSame(s, ((MathNet.Numerics.LinearAlgebra.Double.DenseMatrix)copy[3]).Storage);
            Assert.AreSame(copy[2], s.Data);
            Assert.AreNotSame(data, s.Data);
            CollectionAssert.AreEqual(data, s.Data);
            m[1, 2] = 99;
            Assert.AreEqual(99.0, s.Data[5]);
            Assert.AreEqual(6.0, matrix[1, 2]);
        }

        [TestMethod]
        public void StandaloneElementCopyMatchesIndependentDataContractRoundTrip()
        {
            var nodes = new[] { new NodeElement(new Point3d(1, 2, 3), id: 11), new NodeElement(new Point3d(4, 2, 3), id: 22), new NodeElement(new Point3d(1, 8, 3), id: 33) };
            var shell = new AreaElement(null, null, id: 44, name: "S");
            shell.ConnectNodes(nodes); shell.Assignments.Offset = 2; shell.Assignments.ReinforcementZone = "zone";
            var serializer = new DataContractSerializer(typeof(AreaElement), new DataContractSerializerSettings
            { KnownTypes = ModelValues.DataContracts, PreserveObjectReferences = true, MaxItemsInObjectGraph = 100000 });
            AreaElement expected;
            using (var stream = new MemoryStream())
            { serializer.WriteObject(stream, shell); stream.Position = 0; expected = (AreaElement)serializer.ReadObject(stream); }
            var actual = ModelValues.CopyValue(shell);
            string Graph(AreaElement value)
            {
                using (var stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, value);
                    return ContractGraph(Encoding.UTF8.GetString(stream.ToArray()));
                }
            }
            // Fingerprinting an Element alone compares its identity. Compare the full independent
            // XML graph here. Entity GUIDs are checked below; geometry GUIDs regenerate.
            // XML normalizes signed zero and reconstructs immutable strings independently.
            Assert.AreEqual(Graph(expected), Graph(actual));
            Assert.AreEqual(expected.Guid, actual.Guid);
            CollectionAssert.AreEqual(expected.Nodes.Select(n => n.Guid).ToArray(), actual.Nodes.Select(n => n.Guid).ToArray());
        }

        private static string ContractGraph(string xml)
        {
            XNamespace z = "http://schemas.microsoft.com/2003/10/Serialization/";
            XNamespace i = "http://www.w3.org/2001/XMLSchema-instance";
            var root = XElement.Parse(xml);
            var ids = root.DescendantsAndSelf().Where(e => e.Attribute(z + "Id") != null)
                .ToDictionary(e => (string)e.Attribute(z + "Id"));
            var objects = new Dictionary<string, int>();
            var result = new StringBuilder();
            void Visit(XElement element)
            {
                result.Append('[').Append(element.Name);
                var reference = (string)element.Attribute(z + "Ref");
                var content = reference == null ? element : ids[reference];
                var id = (string)content.Attribute(z + "Id");
                bool scalar = !content.HasElements && content.Attribute(z + "Size") == null;
                if (!scalar && id != null)
                {
                    if (objects.TryGetValue(id, out var ordinal))
                    { result.Append(" ref=").Append(ordinal).Append(']'); return; }
                    objects.Add(id, objects.Count);
                    result.Append(" id=").Append(objects[id]);
                }
                var type = (string)content.Attribute(i + "type");
                if (type != null)
                {
                    var parts = type.Split(':');
                    result.Append(" type=").Append(parts.Length == 1 ? content.GetDefaultNamespace() : content.GetNamespaceOfPrefix(parts[0])).Append(':').Append(parts.Last());
                }
                if (content.Attribute(z + "Size") != null) result.Append(" size=").Append((string)content.Attribute(z + "Size"));
                if ((string)content.Attribute(i + "nil") == "true") result.Append(" nil");
                if (scalar && content.Name.LocalName != "Guid")
                    result.Append('=').Append(content.Value == "-0" && (type?.EndsWith(":double") == true || content.Name.LocalName == "double") ? "0" : content.Value);
                foreach (var child in content.Elements()) Visit(child);
                result.Append(']');
            }
            Visit(root);
            return result.ToString();
        }
    }
}
