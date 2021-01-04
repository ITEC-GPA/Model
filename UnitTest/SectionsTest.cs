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
        public void SectionCHSTest()
        {
            double d = 400;
            double t = 10;
            double di = d - 2.0 * t;
            bool isColdFormed = true;
            Section sec = new SectionCHS(d, t, new SteelMaterial("S355", 200000, 0.3, 355, 510, 7850), isColdFormed);

            Point2d centroid = new Point2d(d / 2, d / 2);
            Point2d shearCenter = centroid;
            double A = Math.PI * (d * d - di * di) / 4.0;
            double J22 = Math.PI * (Math.Pow(d, 4) - Math.Pow(di, 4)) / 64.0;
            double Wel2 = J22 / (d / 2.0);
            double Wpl2 = (Math.Pow(d, 3.0) - Math.Pow(di, 3.0)) / 6.0;
            double Jt = Math.PI * (Math.Pow(d, 4.0) - Math.Pow(di, 4.0)) / (32.0);
            double Jw = 0;
            double i = Math.Sqrt(J22 / A);

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
        public void SectionRHSTest()
        {
            double h = 200;
            double b = 100;
            double tf = 20;
            double tw = 10;
            SectionRHS sec = new SectionRHS(h, b, tf, tf, tw, tw, false, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850));

            double A = 7200;
            double J2 = 39360000;
            double J1 = 9840000;
            double Wel2 = 393600;
            double Wel1 = 196800;
            double Wpl2 = 488000;
            double Wpl1 = 244000;
            double Jt = 23328000;
            
            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(sec.Centroid.X, b / 2.0); //only with tftop = tfbottom && tw1 == tw2
            Assert.AreEqual(sec.Centroid.Y, h / 2.0); //only with tftop = tfbottom && tw1 == tw2
            Assert.AreEqual(b - 2 * tw, sec.Bint);
            Assert.AreEqual(h - 2 * tf, sec.Hw);
            Assert.AreEqual(J2, sec.J22);
            Assert.AreEqual(J1, sec.J11);
            Assert.AreEqual(Wel2, sec.Wel22Min);
            Assert.AreEqual(Wel1, sec.Wel11Min);
            Assert.AreEqual(Wpl2, sec.Wpl22, 1);
            Assert.AreEqual(Wpl1, sec.Wpl11, 1);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.01);
        }

        [TestMethod]
        public void SectionHTest()
        {
            double h = 300;
            double tw = 15;
            double bt = 100;
            double bb = 200;
            double tt = 20;
            double tb = 10;
            SectionH sec = new SectionH(h, tw, bt, tt, bb, tb, true, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850));

            double A = 8050;
            double J2 = 1.059*1e8;
            double J1 = 8409271;
            double Wel2 = 688906.1;
            double Wel1 = 84092.71;
            double Wpl2 = 843375;
            double Wpl1 = 165187.5;
            double Jt = 637083.3; //Straus = 637083.3 vs SAP = 590752 So different!

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(J2 / sec.J22) -1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J1 / sec.J11) -1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / sec.Wel22Min) -1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel1 / sec.Wel11Min) -1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / sec.Wpl22) -1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl1 / sec.Wpl11) -1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.03);
        }

        [TestMethod]
        public void SectionAsymmetricHTest1()
        {
            double h = 400;
            double tw = 12;
            double bt = 200;
            double bb = 300;
            double tt = 10;
            double tb = 25;
            SectionH sec = new SectionH(h, tw, bt, tt, bb, tb, true, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850));

            double A = 13880;
            double J2 = 3.193 * 1e8;
            double J1 = 62969227;
            double Wel2 = 1178986;
            double Wel1 = 419794;
            double Wpl2 = 1632054;
            double Wpl1 = 675640;
            double JtSAP = 1750920;
            double JtStraus = 1839406.666667;
            double Jt = (JtSAP + JtStraus) / 2.0;
            //double JwSAP = 1.317 * 1e-6 * Math.Pow(1000.0,6); //conversion

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(J2 / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J1 / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / sec.Wel22Min) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel1 / sec.Wel11Min) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / sec.Wpl22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl1 / sec.Wpl11) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.03);
            //Assert.AreEqual(JwSAP / sec.Jw - 1, 0, 0.05);
        }

        [TestMethod]
        public void SectionTTest()
        {
            double h = 400;
            double b = 200;
            double tf = 10;
            double tw = 50;
            SectionT sec = new SectionT(h, b, tw, tf, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850));

            double A = 21500;
            double J2 = 3.197*1e8;
            double J1 = 10729167;
            double Wel2 = 1496864.9;
            double Wel1 = 107291.67;
            double Wpl2 = 2281250;
            double Wpl1 = 343750;
            double Jt = 16316666.66667; //Straus : 16316666.66667 | Sap: 15845817

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(J2 / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J1 / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / sec.Wel22Min) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel1 / sec.Wel11Min) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / sec.Wpl22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl1 / sec.Wpl11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.03);
        } 
    }
}
