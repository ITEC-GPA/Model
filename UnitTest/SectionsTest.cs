using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Sections;
using GPC.Model.Materials;
using GPC.Geometry;
using GPC.TestUtilities;
using GPC.Model.Sections.Steel;

namespace ModelObjectTest
{
    [TestClass]
    public class SectionsTest : UnitTestBase
    {
        [TestMethod]
        public void SectionCHS_Test1()
        {
            double d = 400;
            double t = 10;
            double di = d - 2.0 * t;
            SteelSectionCHS sec = new SteelSectionCHS(d, t, new SteelMaterial("S355", 200000, 0.3, 355, 510, 7850),"", Section.FormedTypes.ColdFormed);

            Point2d centroid = new Point2d(d / 2, d / 2);
            Point2d shearCenter = centroid;
            double A = Math.PI * (d * d - di * di) / 4.0;
            double J = Math.PI * (Math.Pow(d, 4) - Math.Pow(di, 4)) / 64.0;
            double Wel2 = J / (d / 2.0);
            double Wpl2 = (Math.Pow(d, 3.0) - Math.Pow(di, 3.0)) / 6.0;
            double Jt = Math.PI * (Math.Pow(d, 4.0) - Math.Pow(di, 4.0)) / (32.0);
            double Jw = 0;
            double i = Math.Sqrt(J / A);

            Assert.AreEqual(0, sec.AngleX1);
            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(centroid, sec.Centroid);
            Assert.AreEqual(i, sec.InertiaRadiusX);
            Assert.AreEqual(i, sec.InertiaRadiusY);
            Assert.AreEqual(true, sec.IsDoubleSymmetric);
            Assert.AreEqual(true, sec.IsSymmetricAlongYLocalAxis);
            Assert.AreEqual(true, sec.IsSymmetricAlongZLocalAxis);
            Assert.AreEqual(J, sec.Jxx);
            Assert.AreEqual(J, sec.Jyy);
            Assert.AreEqual(Jt, sec.Jt);
            Assert.AreEqual(Jw, sec.Jw);
            Assert.AreEqual(shearCenter, sec.ShearCenter);
            Assert.AreEqual(Wel2, sec.CalculateWel());
            Assert.AreEqual(Wpl2, sec.CalculateWpl());
        }

        [TestMethod]
        public void SectionCHS_Test2()
        {
            double d = 250;
            double t = 5;
            SteelSectionCHS sec = new SteelSectionCHS(d, t, new SteelMaterial("S355", 200000, 0.3, 355, 510, 7850), "", Section.FormedTypes.ColdFormed);

            Point2d centroid = new Point2d(d / 2, d / 2);
            Point2d shearCenter = centroid;
            double A = 3848.45;
            double J = 2.89*1e7;
            double Wel2 = 2.31*1e5;
            double Wpl2 = 3*1e5;
            double Jt = 57774871;
            double Jw = 0;
            double i = Math.Sqrt(J / A);

            Assert.AreEqual(0, sec.AngleX1);   
            Assert.AreEqual(centroid, sec.Centroid);
            Assert.AreEqual(Math.Abs(A / sec.Area) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(i / sec.InertiaRadiusX) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(i / sec.InertiaRadiusY) - 1, 0, 0.001);
            Assert.IsTrue(sec.IsDoubleSymmetric);
            Assert.IsTrue(sec.IsSymmetricAlongYLocalAxis);
            Assert.IsTrue(sec.IsSymmetricAlongZLocalAxis);
            Assert.AreEqual(shearCenter, sec.ShearCenter);
            Assert.AreEqual(Math.Abs(J / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jw - sec.Jw), 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / sec.CalculateWel()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / sec.CalculateWpl()) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionCHS_Test3()
        {
            double d = 350;
            double t = 6;
            SteelSectionCHS sec = new SteelSectionCHS(d, t, new SteelMaterial("S355", 200000, 0.3, 355, 510, 7850), "", Section.FormedTypes.ColdFormed);

            Point2d centroid = new Point2d(d / 2, d / 2);
            Point2d shearCenter = centroid;
            double A = 6484;
            double J = 9.59 * 1e7;
            double Wel2 = 5.48 * 1e5;
            double Wpl2 = 7.10 * 1e5;
            double Jt = 191888328;
            double Jw = 0;
            double i = Math.Sqrt(J / A);

            Assert.AreEqual(0, sec.AngleX1);
            Assert.AreEqual(centroid, sec.Centroid);
            Assert.AreEqual(Math.Abs(A / sec.Area) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(i / sec.InertiaRadiusX) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(i / sec.InertiaRadiusY) - 1, 0, 0.001);
            Assert.IsTrue(sec.IsDoubleSymmetric);
            Assert.IsTrue(sec.IsSymmetricAlongYLocalAxis);
            Assert.IsTrue(sec.IsSymmetricAlongZLocalAxis);
            Assert.AreEqual(shearCenter, sec.ShearCenter);
            Assert.AreEqual(Math.Abs(J / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jw - sec.Jw), 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / sec.CalculateWel()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / sec.CalculateWpl()) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionCHS_Test4()
        {
            double d = 500;
            double t = 8;
            SteelSectionCHS sec = new SteelSectionCHS(d, t, new SteelMaterial("S355", 200000, 0.3, 355, 510, 7850), "", Section.FormedTypes.ColdFormed);

            Point2d centroid = new Point2d(d / 2, d / 2);
            Point2d shearCenter = centroid;
            double A = 12365;
            double J = 3.74E+08;
            double Wel2 = 1.4970E+06;
            double Wpl2 = 1.9367E+06;
            double Jt = 748496865;
            double Jw = 0;
            double i = Math.Sqrt(J / A);

            Assert.AreEqual(0, sec.AngleX1);
            Assert.AreEqual(centroid, sec.Centroid);
            Assert.AreEqual(Math.Abs(A / sec.Area) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(i / sec.InertiaRadiusX) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(i / sec.InertiaRadiusY) - 1, 0, 0.001);
            Assert.IsTrue(sec.IsDoubleSymmetric);
            Assert.IsTrue(sec.IsSymmetricAlongYLocalAxis);
            Assert.IsTrue(sec.IsSymmetricAlongZLocalAxis);
            Assert.AreEqual(shearCenter, sec.ShearCenter);
            Assert.AreEqual(Math.Abs(J / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jw - sec.Jw), 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / sec.CalculateWel()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / sec.CalculateWpl()) - 1, 0, 0.001);
        }

        /*[TestMethod]
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
        }*/

        [TestMethod]
        public void SectionRHS_Test1()
        {
            double h = 200;
            double b = 100;
            double tf = 20;
            double tw = 10;
            SteelSectionRHS sec = new SteelSectionRHS(h, b, tf, tf, tw, tw,  new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 7200;
            double Jx = 39360000;
            double Jy = 9840000;
            double Welx = 393600;
            double Wely = 196800;
            double Wplx = 488000;
            double Wply = 244000;
            double Jt = 23328000;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(b - 2 * tw, sec.Binternal);
            Assert.AreEqual(h - 2 * tf, sec.Hinternal);
            Assert.AreEqual(Math.Abs(Jy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWelyMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWelxMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWplyy()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWplxx()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.01);
        }

        [TestMethod]
        public void SectionRHS_Test2()
        {
            double h = 400;
            double b = 200;
            double tf = 10;
            double tw = 10;
            SteelSectionRHS sec = new SteelSectionRHS(h, b, tf, tf, tw, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 11600;
            double Jx = 243586666.67;
            double Jy = 81986666.67;
            double Wely = 819866.67;
            double Welx = 1217933.33;
            double Wplx = 1502000.000;
            double Wply = 922000.000;
            double Jt = 189338275.9;

            double a = sec.CalculateWplyy();
            double b2 = sec.CalculateWplxx();

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(b - 2 * tw, sec.Binternal);
            Assert.AreEqual(h - 2 * tf, sec.Hinternal);
            Assert.AreEqual(Math.Abs(Jy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWelyMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWelxMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWplyy()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWplxx()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.01);
        }

        [TestMethod]
        public void SectionRHS_Test3()
        {
            double h = 300;
            double b = 300;
            double tf = 15;
            double tw = 15;
            SteelSectionRHS sec = new SteelSectionRHS(h, b, tf, tf, tw, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 17100;
            double Jx = 232132500;
            double Jy = 232132500;
            double Wely = 1547550;
            double Welx = 1547550;
            double Wplx = 1829250;
            double Wply = 1829250;
            double Jt = 347236875;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(b - 2 * tw, sec.Binternal);
            Assert.AreEqual(h - 2 * tf, sec.Hinternal);
            Assert.AreEqual(Math.Abs(Jy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWelyMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWelxMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWplyy()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWplxx()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.01);
        }

        [TestMethod]
        public void SectionHAsymmetric_Test1()
        {
            double h = 400;
            double tw = 12;
            double bt = 200;
            double bb = 300;
            double tt = 10;
            double tb = 25;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 13880;
            double Jxx = 3.193 * 1e8;
            double Jyy = 62969227;
            double Welx = 1178986;
            double Wely = 419794;
            double Wplx = 1632054;
            double Wply = 675640;
            double JtSAP = 1750920;
            double JtStraus = 1839406.666667;
            double Jt = (JtSAP + JtStraus) / 2.0;
            //double JwSAP = 1.317 * 1e12; //ERRATO
            double JwLTBEAM = 872110 * 1e6;
            double JtCalc = 1849487;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWelyBottom()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWelxTop()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWply()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWplx()) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.03);
            Assert.AreEqual(JtCalc / sec.Jt - 1.0, 0, 0.001);
            Assert.AreEqual(JwLTBEAM / sec.Jw - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionHAsymmetric_Test2()
        {
            double h = 500;
            double tw = 14;
            double bt = 300;
            double bb = 400;
            double tt = 15;
            double tb = 25;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 20940;
            double Jxx = 868212376.6714;
            double Jyy = 167197666.6667;
            double WelxMin = 2802779;
            double WelyMin = 835988;
            double Wplx = 3694171;
            double Wply = 1360040;
            double Jt = 2859873;
            double Jw = 6.205 * 1e12;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWelxMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWelyMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWply()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWplx()) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.001);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionHSymmetric_Test1()
        {
            double h = 400;
            double tw = 16;
            double bt = 400;
            double bb = 400;
            double tt = 25;
            double tb = 25;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 25600;
            double Jxx = 761333333.3333;
            double Jyy = 266803200.0000;
            double WelxMin = 3806667;
            double WelyMin = 1334016;
            double Wplx = 4240000;
            double Wply = 2022400;
            double Jt = 4678667;
            double Jw = 9.375 * 1e12;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWelxMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWelyMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWply()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWplx()) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.001);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionHSymmetric_Test2()
        {
            double h = 500;
            double tw = 14;
            double bt = 350;
            double bb = 350;
            double tt = 25;
            double tb = 25;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 23800;
            double Jxx = 1094333333.3333;
            double Jyy = 178760166.6667;
            double WelxMin = 4377333;
            double WelyMin = 1021487;
            double Wplx = 4865000;
            double Wply = 1553300;
            double Jt = 4080300;
            double Jw = 1.008 * 1e13;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWelxMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWelyMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWply()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWplx()) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.001);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionT_Test()
        {
            double h = 400;
            double b = 200;
            double tf = 10;
            double tw = 50;
            SectionT sec = new SectionT(h, b, tw, tf, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 21500;
            double Jxx = 3.197*1e8;
            double Jyy = 10729167;
            double Welx = 1496864.9;
            double Wely = 107291.67;
            double Wplx = 2281250;
            double Wply = 343750;
            double Jt = 16316666.66667; //Straus : 16316666.66667 | Sap: 15845817

            double a = sec.CalculateWelyRight();
            double b1 = sec.CalculateWelyLeft();
            double c = sec.CalculateWelxTop();
            double d = sec.CalculateWelxBottom();
            double e = sec.CalculateWply();
            double f = sec.CalculateWplx();

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWelyMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWelxMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWply()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWplx()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.03);
        }

        [TestMethod]
        public void SectionT_Test2()
        {
            double h = 400;
            double b = 200;
            double tf = 10;
            double tw = 20;
            SectionT sec = new SectionT(h, b, tw, tf, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            //double JwLTBEAM = 39044 * 1e6; // --> WRONG
            double JwStraus = 1.32284 * 1e10;
            //double JwSAP = 13751083333;
            Assert.AreEqual(JwStraus / sec.Jw - 1.0, 0, 0.05);
        }

        [TestMethod]
        public void SectionC_Test1()
        {
            double h = 400;
            double tw = 10;
            double b = 200;
            double tf = 25;
            SectionC sec = new SteelSectionC(h, tw, b, tf, b, tf, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 13500;
            double Jyy = 56760648.14815;
            double Jxx = 387812500.0;
            double Wely = 455434;
            double Welx = 1939062;
            double Wply = 771250;
            double Wplx = 2181250;
            double JtSAP = 2033837;
            double JtStraus = 2200000.0;
            double Jt = (JtSAP + JtStraus) / 2.0;
            double JwSAP = 1.465E+12;

            Assert.AreEqual(Math.Abs(A / sec.Area) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWelyyMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWelxxMin()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWplyy()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWplxx()) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.03);
            Assert.AreEqual(JwSAP / sec.Jw - 1, 0, 0.07);
        }

        [TestMethod]
        public void SectionL_Test1()
        {
            double h = 500;
            double tw = 40;
            double b = 40.01;
            double tb = 40;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double Wel1 = 1.0 / 6.0 * h * tw*tw;
            double Wel2 = 1.0 / 6.0 * tw * h * h;

            Assert.AreEqual(Math.Abs(Wel2 / sec.CalculateWel22Min()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel1 / sec.CalculateWel11Min()) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionL_Test2()
        {
            double h = 40;
            double tw = 40;
            double b = 500;
            double tb = 40;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double Wel1 = 1.0 / 6.0 * b * tb * tb;
            double Wel2 = 1.0 / 6.0 * tb * b * b;

            Assert.AreEqual(Math.Abs(Wel2 / sec.CalculateWel22Min()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel1 / sec.CalculateWel11Min()) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionL_Test3()
        {
            double h = 500;
            double tw = 40;
            double b = 500;
            double tb = 80;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 56800;
            double J1 = 517472668.5153;
            double J2 = 1951689772.799;
            //double Wel1 = 2368295.77;
            //double Wel2 = 5465136.457;

            double JtSAP = 86856533.3;
            double JtStraus = 89173333.33333;
            double Jt = (JtSAP + JtStraus) / 2.0;
            //double JwSAP = 3.696E+11; --> Wrong
            double JwStraus = 1.64361e12;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(J2 / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J1 / sec.Jxx) - 1, 0, 0.001);
            /*Assert.AreEqual(Math.Abs(Wel2 / sec.Wel22Min) - 1, 0, 0.001); --> SAP ERRATO
            Assert.AreEqual(Math.Abs(Wel1 / sec.Wel11Min) - 1, 0, 0.001); --> SAP ERRATO*/
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.05);
            Assert.AreEqual(JwStraus / sec.Jw - 1, 0, 0.06);
        }
    }
}
