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
        public void CollectionTest1()
        {
            ModelObject.ModelObjectNameEqualityComparer comparer = new ModelObject.ModelObjectNameEqualityComparer();

            ModelObjectCollection<GhostElement> moc = new ModelObjectCollection<GhostElement>(comparer);


            moc.Add(new GhostElement(1, "E1"));
            moc.Add(new GhostElement(2, "E2"));
            moc.Add(new GhostElement(3, "E3"));
            moc.Add(new GhostElement(4, "E3"));


            Assert.AreEqual(moc.Count, 3);
        }


    }
}
