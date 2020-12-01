using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Sections;
using GPC.Model.Materials;

namespace UnitTest
{
    [TestClass]
    public class SectionRectangularTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            double h = 100;
            double b = 10;
            SectionRectangular sec = new SectionRectangular(b, h, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850));

            double A = h * b;
            double J2 = 1.0 / 12.0 * b * Math.Pow(h, 3.0);
            double J1 = 1.0 / 12.0 * h * Math.Pow(b, 3.0);
            double Wel2 = 1.0 / 6.0 * b * Math.Pow(h, 2.0);
            double Wel1 = 1.0 / 6.0 * h * Math.Pow(b, 2.0);
            double Wpl2 = A / 2.0 * h / 2.0;
            double Wpl1 = A / 2.0 * b / 2.0;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(J2, sec.J22);
            Assert.AreEqual(J1, sec.J11);
            Assert.AreEqual(Wel2, sec.Wel22Top);
            Assert.AreEqual(Wel2, sec.Wel22Bottom);
            Assert.AreEqual(Wel1, sec.Wel11Left);
            Assert.AreEqual(Wel1, sec.Wel11Right);
            Assert.AreEqual(Wpl2, sec.Wpl22);
            Assert.AreEqual(Wpl1, sec.Wpl11);
        }
    }
}
