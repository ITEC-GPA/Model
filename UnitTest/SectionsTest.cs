using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Sections;
using GPC.Model.Materials;
using GPC.Geometry;

namespace UnitTest
{
    [TestClass]
    public class SectionsTest
    {
        [TestMethod]
        public void SectionRectangularTest()
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
            Assert.AreEqual(Wel2, sec.Wel22Min);
            Assert.AreEqual(Wel1, sec.Wel11Min);
            Assert.AreEqual(Wpl2, sec.Wpl22);
            Assert.AreEqual(Wpl1, sec.Wpl11);
        }

        [TestMethod]
        public void SectionCHSTest()
        {
            double d = 100;
            double t = 10;
            double di = d - 2.0 * t;
            bool isColdFormed = true;
            Section sec = new SectionCHS(d, t, new SteelMaterial("S355", 200000, 0.3, 355, 510, 7850), isColdFormed);

            Point2d centroid = new Point2d(d / 2, d / 2);
            Point2d shearCenter = centroid;
            double A = Math.PI * (d*d - di * di) /4.0;
            double J22 = Math.PI * (Math.Pow(d,4) - Math.Pow(di, 4)) / 64.0;
            double Wel2 = J22 / (d/2.0);
            double Wpl2 = (Math.Pow(d,3.0) - Math.Pow(di, 3.0)) /6.0;
            double Jt = Math.PI * (Math.Pow(d, 4.0) - Math.Pow(di, 4.0)) / (32.0);
            double Jw = 0;
            double i = Math.Sqrt(J22/A);

            Assert.AreEqual(0, sec.AngleX1);
            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(centroid, sec.Centroid);
            Assert.AreEqual(i, sec.InertiaRadius1);
            Assert.AreEqual(i, sec.InertiaRadius2);
            Assert.AreEqual(true, sec.IsDoubleSymmetric);
            Assert.AreEqual(true, sec.IsSymmetricAlongYLocalAxis);
            Assert.AreEqual(true, sec.IsSymmetricAlongZLocalAxis);
            Assert.AreEqual(J22, sec.J11);
            Assert.AreEqual(J22, sec.J22);
            Assert.AreEqual(Jt, sec.Jt);
            Assert.AreEqual(Jw, sec.Jw);
            Assert.AreEqual(shearCenter, sec.ShearCenter);
            Assert.AreEqual(Wel2, sec.Wel11Min);
            Assert.AreEqual(Wel2, sec.Wel22Min);
            Assert.AreEqual(Wpl2, sec.Wpl11);
            Assert.AreEqual(Wpl2, sec.Wpl22);
        }

        [TestMethod]
        public void SectionTTest()
        {
            double h = 50;
            double b = 60;
            double tf = 5;
            double tw = 10;
            SectionT sec = new SectionT(h, b, tw, tf, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850));

            double A = 750;
            /*double J2 = ;
            double J1 = ;
            double Wel2 = ;
            double Wel1 = ;
            double Wpl2 = ;
            double Wpl1 = ;*/

            Point2d centroid = new Point2d(30, 32.5);

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(sec.Centroid, centroid);
            /*Assert.AreEqual(J2, sec.J22);
            Assert.AreEqual(J1, sec.J11);
            Assert.AreEqual(Wel2, sec.Wel22Min);
            Assert.AreEqual(Wel1, sec.Wel11Min);
            Assert.AreEqual(Wpl2, sec.Wpl22);
            Assert.AreEqual(Wpl1, sec.Wpl11);*/
        }

        [TestMethod]
        public void SectionRHSTest()
        {
            double h = 100;
            double b = 60;
            double t = 5;
            SectionRHS sec = new SectionRHS(h, b, t, t, t, t, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850));

            double A = 1500;
            double J2 = 1962500;
            double J1 = 862500;
            double Wel2 = 39250;
            double Wel1 = 28750;
            double Wpl2 = 48750;
            double Wpl1 = 33750;
            double Jt = 1820041;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(J2, sec.J22);
            Assert.AreEqual(J1, sec.J11);
            Assert.AreEqual(Wel2, sec.Wel22Min);
            Assert.AreEqual(Wel1, sec.Wel11Min);
            Assert.AreEqual(Wpl2, sec.Wpl22);
            Assert.AreEqual(Wpl1, sec.Wpl11);
            Assert.AreEqual(Math.Abs(Jt/sec.Jt)-1.0, 0, 0.01);
        }

        [TestMethod]
        public void SectionHTest1()
        {
            double h = 500;
            double tw = 10;
            double b = 400;
            double tf = 10;
            SectionH sec = new SectionH(h, tw, b, tf, b, tf, true, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850));
            double A = sec.Area;
            Point2d centroid = sec.Centroid;
            double J11 = sec.J11;
            double J22 = sec.J22;
            double wply = sec.Wpl22;
            double wplz = sec.Wpl11;

            var x = "";
        }
    }
}
