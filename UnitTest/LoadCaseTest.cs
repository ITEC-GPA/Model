using GPC.Model.LoadCases;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace ModelObjectTest
{
    [TestClass]
    public class LoadCaseTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void CleanUp()
        {
            if (Directory.Exists(TestContext.TestDir))
                Directory.Delete(TestContext.TestDir, true);
        }


        [TestMethod]
        public void LoadCaseTest1()
        {
            Guid g = Guid.NewGuid();

            LoadCase lc = new LoadCase("Snow", LoadCase.LoadCaseType.Snow, g);

            LoadCase lc1 = new LoadCase("Snow", LoadCase.LoadCaseType.Snow, g);
            LoadCase lc2 = new LoadCase("Wind", LoadCase.LoadCaseType.Wind, Guid.NewGuid());

            Assert.IsTrue(lc.Equals(lc1));
            Assert.IsFalse(lc.Equals(lc2));
        }


        [TestMethod]
        public void LoadCaseTest2()
        {
            Guid g = Guid.NewGuid();

            LoadCasePrEn lc = new LoadCasePrEn("Snow", LoadCase.LoadCaseType.Snow, LoadCasePrEn.LoadCasePrEnType.SnowCanopies, g);

            LoadCasePrEn lc1 = new LoadCasePrEn("Snow", LoadCase.LoadCaseType.Snow, LoadCasePrEn.LoadCasePrEnType.SnowCanopies, g);
            LoadCasePrEn lc2 = new LoadCasePrEn("Wind", LoadCase.LoadCaseType.Wind, LoadCasePrEn.LoadCasePrEnType.BalustradeDuty, Guid.NewGuid());

            Assert.IsTrue(lc.Equals(lc1));
            Assert.IsFalse(lc.Equals(lc2));
        }
    }
}
