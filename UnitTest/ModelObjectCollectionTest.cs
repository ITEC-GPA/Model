using System.ComponentModel;
using GPC.Model;
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

        
        [TestMethod]
        public void KeyValuePairHashTest1()
        {

            KeyValuePairHashedCollection<GhostElement, GhostElement> collection = new KeyValuePairHashedCollection<GhostElement, GhostElement>();

            collection.Add(new GhostElement(1, "1"), new GhostElement(2));
            collection.Add(new GhostElement(1, "1"), new GhostElement(2));
            collection.Add(new GhostElement(1, "1"), new GhostElement(3));
            collection.Add(new GhostElement(2, "2"), new GhostElement(2));

            Assert.AreEqual(collection.Count, 2);

            collection.Remove(new GhostElement(1, "1"));
            Assert.AreEqual(collection.Count, 1);

        }

        [TestMethod]
        public void KeyValuePairHashTest2()
        {

            KeyValuePairHashedCollection<TestElement, TestElement> collection = new KeyValuePairHashedCollection<TestElement, TestElement>();

            var a = new TestElement(1);
            var b = new TestElement(2);

            collection.Add(a, new TestElement(2));
            collection.Add(a, new TestElement(2));
            collection.Add(a, new TestElement(3));
            collection.Add(b, new TestElement(2));

            Assert.AreEqual(collection.Count, 2);

            b.ID = 1;

            Assert.AreEqual(collection.Count, 2);

            Assert.AreEqual(new TestElement(3), collection.GetValue(b));            

        }

        private class TestElement : Element, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public TestElement(int iD)
            {
                ID = iD;
            }

            public int ID { get; set; }

            public override bool Equals(object obj)
            {
                return obj is TestElement element &&
                       ID == element.ID;
            }

            public override int GetHashCode()
            {
                return 1213502048 + ID.GetHashCode();
            }
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
