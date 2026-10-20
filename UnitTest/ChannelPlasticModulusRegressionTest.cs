using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ModelObjectTest
{
    [TestClass]
    public class ChannelPlasticModulusRegressionTest
    {
        [DataTestMethod]
        [DataRow(300d, 10d, 100d, 15d)]
        [DataRow(300d, 20d, 100d, 15d)]
        [DataRow(300d, 5d, 100d, 15d)]
        [DataRow(200d, 10d, 110d, 10d)]
        public void WeakAxisPlasticModulusBalancesArea(double h, double tw, double b, double tf)
        {
            var channel = new SectionC(h, tw, b, tf, b, tf, "");
            // Independent integration of a full-height web and two outstands.
            double area = h * tw + 2 * (b - tw) * tf;
            double half = area / 2;
            double x = half <= h * tw ? half / h : tw + (half - h * tw) / (2 * tf);
            double totalFirstMoment = h * tw * tw / 2 + tf * (b * b - tw * tw);
            double leftFirstMoment = x <= tw ? h * x * x / 2 :
                h * tw * tw / 2 + tf * (x * x - tw * tw);
            double expected = totalFirstMoment - 2 * leftFirstMoment;
            Assert.AreEqual(expected, channel.WplY, expected * 1e-10);
            Assert.AreEqual(expected, channel.Wpl2, expected * 1e-10);
        }
    }
}
