using GPC.Model.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneralTest
{
    [TestClass]
    public class OperatorsTest
    {
        [TestMethod]
        public void ResultType()
        {
            ResultDisplacement rd1 = new ResultDisplacement(1, 2, 3, 4, 5, 6);
            ResultDisplacement rd2 = null;

            Assert.IsTrue(rd1 != null);
            Assert.IsTrue(rd2 == null);

            Assert.IsFalse(rd1 == null);
            Assert.IsFalse(rd2 != null);
        }
    }
}
