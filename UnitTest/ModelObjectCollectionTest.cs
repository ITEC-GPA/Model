using GPC.Model;
using GPC.Model.Base;
using GPC.Model.Elements;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModelObjectTest
{
    [TestClass]
    public class ModelObjectCollectionTest : UnitTestBase
    {


        [TestMethod]
        public void UniqueNameTest1()
        {
            UniqueNameCollection<GhostElement> moc = new UniqueNameCollection<GhostElement>
            {
                new GhostElement(1, "E1"),
                new GhostElement(2, "E2"),
                new GhostElement(3, "E3"),
                new GhostElement(4, "E3")
            };


            Assert.AreEqual(moc.Count, 3);
        }


        [TestMethod]
        public void KeyValuePairTest1()
        {

            KeyValuePairCollection<GhostElement, GhostElement> collection = new KeyValuePairCollection<GhostElement, GhostElement>();

            collection.Add(new GhostElement("1"), new GhostElement(2));
            collection.Add(new GhostElement("1"), new GhostElement(2));
            collection.Add(new GhostElement("1"), new GhostElement(3));

            Assert.AreEqual(collection.Count, 3);
        }

        [TestMethod]
        public void KeyValuePairTest2()
        {

            KeyValuePairCollection<GhostElement, GhostElement> collection = new KeyValuePairCollection<GhostElement, GhostElement>();

            collection.AddUnique(new GhostElement(1, "1"), new GhostElement(2));
            collection.AddUnique(new GhostElement(1, "1"), new GhostElement(2));
            collection.AddUnique(new GhostElement(1, "1"), new GhostElement(3));
            collection.AddUnique(new GhostElement(2, "2"), new GhostElement(2));

            Assert.AreEqual(collection.Count, 2);
            Assert.IsTrue(collection.GetValue(new GhostElement(1, "1")).Id == 3);
            Assert.IsTrue(collection.GetValue(new GhostElement(1, "2")).Id == 2);
        }
    }
}
