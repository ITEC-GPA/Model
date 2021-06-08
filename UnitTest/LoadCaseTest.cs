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

            LoadCaseEn16612 lc = new LoadCaseEn16612("Snow", LoadCase.LoadCaseTypes.Snow, LoadCaseEn16612.LoadCaseEn16612Types.SnowCanopies, g);

            LoadCaseEn16612 lc1 = new LoadCaseEn16612("Snow", LoadCase.LoadCaseTypes.Snow, LoadCaseEn16612.LoadCaseEn16612Types.SnowCanopies, g);
            LoadCaseEn16612 lc2 = new LoadCaseEn16612("Wind", LoadCase.LoadCaseTypes.WindPressure, LoadCaseEn16612.LoadCaseEn16612Types.BalustradeDuty, Guid.NewGuid());

            Assert.IsTrue(lc.Equals(lc1));
            Assert.IsFalse(lc.Equals(lc2));
        }



        [TestMethod]
        public void LoadCaseTest3()
        {
            LoadCase lc = new LoadCase("Snow", LoadCase.LoadCaseTypes.Snow);

            LoadCase lc1 = new LoadCase("Snow", LoadCase.LoadCaseTypes.Snow);
            LoadCase lc2 = new LoadCase("Wind", LoadCase.LoadCaseTypes.WindPressure);

            ClimateLoadCase cls = new ClimateLoadCase("Cls", ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaH, 10, 20);

            
            Assert.IsTrue(lc.Equals(lc1));
            Assert.IsFalse(lc.Equals(lc2));
            Assert.IsFalse(lc.Equals(cls));
        }
    }
}
