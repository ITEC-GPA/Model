using GPC.Model.LoadCases;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GPC.TestUtilities;

namespace ModelObjectTest
{
    [TestClass]
    public class LoadCaseTest : UnitTestBase
    {

        [TestMethod]
        public void LoadCaseTest1()
        {
            Guid g = Guid.NewGuid();

            LoadCase lc = new LoadCase("Snow", LoadCase.LoadCaseTypes.Snow, g);

            LoadCase lc1 = new LoadCase("Snow", LoadCase.LoadCaseTypes.Snow, g);
            LoadCase lc2 = new LoadCase("Wind", LoadCase.LoadCaseTypes.WindPressure, Guid.NewGuid());

            Assert.IsTrue(lc.Equals(lc1));
            Assert.IsFalse(lc.Equals(lc2));
        }


        [TestMethod]
        public void LoadCaseTest2()
        {
            Guid g = Guid.NewGuid();

            LoadCasePrEn lc = new LoadCasePrEn("Snow", LoadCase.LoadCaseTypes.Snow, LoadCasePrEn.LoadCasePrEnType.SnowCanopies, g);

            LoadCasePrEn lc1 = new LoadCasePrEn("Snow", LoadCase.LoadCaseTypes.Snow, LoadCasePrEn.LoadCasePrEnType.SnowCanopies, g);
            LoadCasePrEn lc2 = new LoadCasePrEn("Wind", LoadCase.LoadCaseTypes.WindPressure, LoadCasePrEn.LoadCasePrEnType.BalustradeDuty, Guid.NewGuid());

            Assert.IsTrue(lc.Equals(lc1));
            Assert.IsFalse(lc.Equals(lc2));
        }
    }
}
