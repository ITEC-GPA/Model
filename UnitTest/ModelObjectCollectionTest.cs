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
            UniqueNameCollection<GhostElement> moc = new UniqueNameCollection<GhostElement>();


            moc.Add(new GhostElement(1, "E1"));
            moc.Add(new GhostElement(2, "E2"));
            moc.Add(new GhostElement(3, "E3"));
            moc.Add(new GhostElement(4, "E3"));


            Assert.AreEqual(moc.Count, 3);
        }


    }
}
