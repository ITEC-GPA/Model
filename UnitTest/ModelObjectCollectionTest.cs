using System.ComponentModel;
using System;
using System.Linq;
using GPC.Model;
using GPC.Model.Elements;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Materials;
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


        [TestMethod]
        public void RebarCollectionTest1()
        {
            RebarSectionCircular rebarSection1 = new RebarSectionCircular(10, RebarMaterial.B450C);
            RebarSectionCircular rebarSection2 = new RebarSectionCircular(20, RebarMaterial.B450C);
            RebarSectionCircular rebarSection3 = new RebarSectionCircular(30, RebarMaterial.B450C);
            RebarSectionCircular rebarSection4 = new RebarSectionCircular(40, RebarMaterial.B450C);

            RebarCollection collection = new RebarCollection
            {
                new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(0, 0), 0, 1),
                new ReinforcedConcreteRebar(rebarSection2, new GPC.Geometry.Point2d(0, 0), 0, 2),
                new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(1, 1), 0, 2),
                new ReinforcedConcreteRebar(rebarSection3, new GPC.Geometry.Point2d(2, 2), 0, 2),
                new ReinforcedConcreteRebar(rebarSection4, new GPC.Geometry.Point2d(3, 2))
            };

            foreach (var item in collection)
            {
                Console.WriteLine($"{item.Id} {item.RebarSection.Area}");
            }

            Assert.IsTrue(collection.Count == 4);
        }

        [TestMethod]
        public void RebarCollectionTest2()
        {
            RebarSectionCircular rebarSection1 = new RebarSectionCircular(10, RebarMaterial.B450C);
            RebarSectionCircular rebarSection2 = new RebarSectionCircular(20, RebarMaterial.B450C);

            ReinforcedConcreteRebar rebar1 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(0, 0));
            ReinforcedConcreteRebar rebar2 = new ReinforcedConcreteRebar(rebarSection2, new GPC.Geometry.Point2d(0, 0));

            RebarCollection collection = new RebarCollection
            {
                rebar1,
            };

            collection.Replace(rebar1, rebar2);

            Assert.IsTrue(collection.Count == 1);
            Assert.IsTrue(collection.GetById(1) == rebar2);
        }

        [TestMethod]
        public void RebarCollectionTest3()
        {
            RebarSectionCircular rebarSection1 = new RebarSectionCircular(10, RebarMaterial.B450C);
            RebarSectionCircular rebarSection2 = new RebarSectionCircular(20, RebarMaterial.B450C);

            ReinforcedConcreteRebar rebar11 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(0, 0));
            ReinforcedConcreteRebar rebar12 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(10, 0));
            ReinforcedConcreteRebar rebar13 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(20, 0));
            ReinforcedConcreteRebar rebar14 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(30, 0));
            ReinforcedConcreteRebar rebar15 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(40, 0));
            ReinforcedConcreteRebar rebar16 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(50, 0));
            ReinforcedConcreteRebar rebar2 = new ReinforcedConcreteRebar(rebarSection2, new GPC.Geometry.Point2d(0, 0));

            RebarCollection collection = new RebarCollection
            {
                rebar11,
                rebar12,
                rebar13,
                rebar14,
                rebar15,
                rebar16,
            };

            collection.Replace(rebar11, rebar2);

            Assert.IsTrue(collection.Count == 6);
            Assert.IsTrue(collection.GetById(7) == rebar2);
            Assert.IsTrue(collection.GetById(2) == rebar12);
            Assert.IsTrue(collection.GetById(3) == rebar13);
            Assert.IsTrue(collection.GetById(4) == rebar14);
            Assert.IsTrue(collection.GetById(5) == rebar15);
            Assert.IsTrue(collection.GetById(6) == rebar16);
        }

        [TestMethod]
        public void RebarCollectionTest4()
        {
            RebarSectionCircular rebarSection1 = new RebarSectionCircular(10, RebarMaterial.B450C);
            RebarSectionCircular rebarSection2 = new RebarSectionCircular(20, RebarMaterial.B450C);

            ReinforcedConcreteRebar rebar11 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(0, 0));
            ReinforcedConcreteRebar rebar12 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(10, 0));
            ReinforcedConcreteRebar rebar13 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(20, 0));
            ReinforcedConcreteRebar rebar14 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(30, 0));
            ReinforcedConcreteRebar rebar15 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(40, 0));
            ReinforcedConcreteRebar rebar16 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(50, 0));

            RebarCollection collection = new RebarCollection
            {
                rebar11,
                rebar12,
                rebar13,
                rebar14,
                rebar15,
                rebar16,
            };

            collection.Remove(rebar11);
            collection.Remove(rebar12);
            collection.Remove(rebar13);
            collection.Remove(rebar14);
            collection.Remove(rebar15);
            collection.Remove(rebar16);

            Assert.IsTrue(collection.Count == 0);
        }

        [TestMethod]
        public void RebarCollectionTest5()
        {
            RebarSectionCircular rebarSection1 = new RebarSectionCircular(10, RebarMaterial.B450C);
            RebarSectionCircular rebarSection2 = new RebarSectionCircular(20, RebarMaterial.B450C);

            ReinforcedConcreteRebar rebar11 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(0, 0));
            ReinforcedConcreteRebar rebar12 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(10, 0));
            ReinforcedConcreteRebar rebar13 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(20, 0));
            ReinforcedConcreteRebar rebar14 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(30, 0));
            ReinforcedConcreteRebar rebar15 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(40, 0));
            ReinforcedConcreteRebar rebar16 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(50, 0));
            ReinforcedConcreteRebar rebar2 = new ReinforcedConcreteRebar(rebarSection2, new GPC.Geometry.Point2d(0, 0));

            RebarCollection collection = new RebarCollection
            {
                rebar11,
                rebar12,
                rebar13,
                rebar14,
                rebar15,
                rebar16,
            };

            collection.Replace(rebar11, rebar2);
            collection.Replace(rebar12, rebar2);
            collection.Replace(rebar13, rebar2);
            collection.Replace(rebar14, rebar2);
            collection.Replace(rebar15, rebar2);
            collection.Replace(rebar16, rebar2);

            Assert.IsTrue(collection.Count == 1);
            Assert.IsTrue(collection.GetById(7) == rebar2);
        }

        [TestMethod]
        public void RebarCollectionTest6()
        {
            RebarSectionCircular rebarSection1 = new RebarSectionCircular(10, RebarMaterial.B450C);

            ReinforcedConcreteRebar rebar11 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(0, 0));
            ReinforcedConcreteRebar rebar12 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(10, 0));
            ReinforcedConcreteRebar rebar13 = new ReinforcedConcreteRebar(rebarSection1, new GPC.Geometry.Point2d(20, 0));

            RebarCollection collection = new RebarCollection
            {
                rebar11,
                rebar12,
                rebar13,
            };

            collection.Clear();

            Assert.IsTrue(collection.Count == 0);
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
