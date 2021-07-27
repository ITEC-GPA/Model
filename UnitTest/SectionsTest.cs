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
            Assert.AreEqual(i, sec.R22);
            Assert.AreEqual(i, sec.R11);
            Assert.AreEqual(true, sec.IsDoubleSymmetric);
            Assert.AreEqual(true, sec.IsSymmetricAlongYLocalAxis);
            Assert.AreEqual(true, sec.IsSymmetricAlongXLocalAxis);
            Assert.AreEqual(J, sec.J11);
            Assert.AreEqual(J, sec.J22);
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
            Assert.AreEqual(Math.Abs(i / sec.R22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(i / sec.R11) - 1, 0, 0.001);
            Assert.IsTrue(sec.IsDoubleSymmetric);
            Assert.IsTrue(sec.IsSymmetricAlongYLocalAxis);
            Assert.IsTrue(sec.IsSymmetricAlongXLocalAxis);
            Assert.AreEqual(shearCenter, sec.ShearCenter);
            Assert.AreEqual(Math.Abs(J / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J / sec.J22) - 1, 0, 0.001);
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
            Assert.AreEqual(Math.Abs(i / sec.R22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(i / sec.R11) - 1, 0, 0.001);
            Assert.IsTrue(sec.IsDoubleSymmetric);
            Assert.IsTrue(sec.IsSymmetricAlongYLocalAxis);
            Assert.IsTrue(sec.IsSymmetricAlongXLocalAxis);
            Assert.AreEqual(shearCenter, sec.ShearCenter);
            Assert.AreEqual(Math.Abs(J / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J / sec.J11) - 1, 0, 0.001);
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
            Assert.AreEqual(Math.Abs(i / sec.R22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(i / sec.R11) - 1, 0, 0.001);
            Assert.IsTrue(sec.IsDoubleSymmetric);
            Assert.IsTrue(sec.IsSymmetricAlongYLocalAxis);
            Assert.IsTrue(sec.IsSymmetricAlongXLocalAxis);
            Assert.AreEqual(shearCenter, sec.ShearCenter);
            Assert.AreEqual(Math.Abs(J / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J / sec.J11) - 1, 0, 0.001);
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
            Assert.AreEqual(b - 2 * tw, sec.BaseInternal);
            Assert.AreEqual(h - 2 * tf, sec.Heightinternal);
            Assert.AreEqual(Math.Abs(Jy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.001);
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

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(b - 2 * tw, sec.BaseInternal);
            Assert.AreEqual(h - 2 * tf, sec.Heightinternal);
            Assert.AreEqual(Math.Abs(Jy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.001);
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
            Assert.AreEqual(b - 2 * tw, sec.BaseInternal);
            Assert.AreEqual(h - 2 * tf, sec.Heightinternal);
            Assert.AreEqual(Math.Abs(Jy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.01);
        }

        [TestMethod]
        public void SectionRHS_Test4()
        {
            double h = 150;
            double b = 125;
            double tf = 12.5;
            double tw = 12.5;
            SteelSectionRHS sec = new SteelSectionRHS(h, b, tf, tf, tw, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 6250;
            double Jx = 18880208.33;
            double Jy = 13997395.83;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(b - 2 * tw, sec.BaseInternal);
            Assert.AreEqual(h - 2 * tf, sec.Heightinternal);
            Assert.AreEqual(Math.Abs(Jy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.J11) - 1, 0, 0.001);
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
            double JtSAP = 1750920;                 // noi troviamo il valore di Jt = 1758993;
            double JtStraus = 1839406.666667;       // 
            double JtCalc = 1849487;
            //double JwSAP = 1.317 * 1e12; //ERRATO
            double JwLTBEAM = 872110 * 1e6;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWelyBottom()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWelxTop()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.001);
            Assert.AreEqual(JtSAP / sec.Jt - 1.0, 0, 0.005);
            Assert.AreEqual(JtStraus / sec.Jt - 1.0, 0, 0.05);
            Assert.AreEqual(JtCalc / sec.CalculateJtSSRC1889() - 1.0, 0, 0.001);
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
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.CalculateJtSSRC1889() - 1.0, 0, 0.001);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionHAsymmetric_Test3()
        {
            double h = 500;
            double tw = 12;
            double bt = 300;
            double bb = 500;
            double tt = 10;
            double tb = 40;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 28400;
            double Jxx = 828928638;
            double Jyy = 439238667;
            double WelxMin = 2162952;
            double WelyMin = 1756955;
            double Wplx = 2879887;
            double Wplxsap = 2912720;
            double Wply = 2741200;
            double Jt = 11040267;
            double jtSap = 10481812;
            double jtStraus= 10534103.36714;
            double Jw = 4816472960152;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Wplxsap / sec.CalculateWpl1()) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.CalculateJtSSRC1889() - 1.0, 0, 0.001);
            Assert.AreEqual(jtStraus / sec.Jt - 1.0, 0, 0.001);
            Assert.AreEqual(jtSap / sec.Jt - 1.0, 0, 0.005);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionHAsymmetric_Test4()
        {
            double h = 500;
            double tw = 12;
            double bt = 500;
            double bb = 300;
            double tt = 40;
            double tb = 10;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 28400;
            double Jxx = 828928638;
            double Jyy = 439238667;
            double WelxMin = 2162952;
            double WelyMin = 1756955;
            double Wplx = 2879887;
            double WplxSap = 2912720;
            double Wply = 2741200;
            double Jt = 11040267;
            double jtSap = 10481812;
            double jtStraus = 10534103.36714;
            double Jw = 4816472960152;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(WplxSap / sec.CalculateWpl1()) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.CalculateJtSSRC1889() - 1.0, 0, 0.001);
            Assert.AreEqual(jtStraus / sec.CalculateJt() - 1.0, 0, 0.001);
            Assert.AreEqual(jtSap / sec.CalculateJt() - 1.0, 0, 0.005);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionHAsymmetric_Test5()
        {
            double h = 400;
            double tw = 12;
            double bt = 300;
            double bb = 500;
            double tt = 15;
            double tb = 40;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 28640;
            double Jxx = 608058102;
            double Jyy = 450474267;
            double WelxMin = 2070723;
            double WelyMin = 1801897;
            double Wplx = 2604387;
            double WplxSap = 2635875;
            double Wply = 2849920;
            double JtSSRC = 11218727;
            double JtSap = 10650301;
            double Jw = 4332121588807;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(WplxSap / sec.CalculateWpl1()) - 1, 0, 0.001);
            Assert.AreEqual(JtSSRC / sec.CalculateJtSSRC1889() - 1.0, 0, 0.001);
            Assert.AreEqual(JtSap / sec.CalculateJt() - 1.0, 0, 0.005);
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
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.CalculateJtSSRC1889() - 1.0, 0, 0.001);
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
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.CalculateJtSSRC1889() - 1.0, 0, 0.001);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionHSymmetric_Test3()
        {
            double h = 600;         // sezione da catalogo ArcelorMittal pagina 64/65 HD400x1299
            double tw = 100;
            double bt = 476;
            double bb = 476;
            double tt = 140;
            double tb = 140;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 165470;
            double Jxx = 7549500000;
            double Jyy = 2544100000;
            double WelxMin = 25160000;
            double WelyMin = 10680000;
            double Wplx = 33260000;
            double Wply = 16670000;
            double jtSap = 7.95 * 1e8;
            double Jt = 955200000;
            double Jw = 133120000000000;

            Assert.AreEqual(Math.Abs(A/sec.Area) -1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.0015);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.16);
            Assert.AreEqual(jtSap / sec.Jt - 1.0, 0, 0.05);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.011);
        }

        [TestMethod]
        public void SectionHSymmetric_Test4()
        {
            double h = 1093;         // sezione da catalogo ArcelorMittal pagina 62/63 HL920x1377
            double tw = 76.7;
            double bt = 473;
            double bb = 473;
            double tt = 115.1;
            double tb = 115.1;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 175370;          // da catalogo (tiene in conto anche i raggi)
            double Jxx = 30354000000;
            double Jyy = 2063500000;
            double WelxMin = 55540000;
            double WelyMin = 8725000;
            double Wplx = 67740000;
            double Wply = 14160000;
            double jtSap = 7.95 * 1e8;
            double Jt = 6.045 * 1e8;
            double Jw = 485320000000000;

            Assert.AreEqual(Math.Abs(A / sec.Area) - 1, 0, 0.0018);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.0033);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWel1()) - 1, 0, 0.0035);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWel2()) - 1, 0, 0.0035);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.0011);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.0035);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.12);
            Assert.AreEqual(jtSap / sec.Jt - 1.0, 0, 0.50);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.016);
        }

        [TestMethod]
        public void SectionHSymmetric_Test5()
        {
            double h = 360;         // sezione da catalogo ArcelorMittal pagina 48/49 IPE360
            double tw = 8.0;
            double bt = 170;
            double bb = 170;
            double tt = 12.7;
            double tb = 12.7;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 7270;          // da catalogo (tiene in conto anche i raggi)
            double Jxx = 162600000;
            double Jyy = 10430000;
            double WelxMin = 903600;
            double WelyMin = 122700;
            double Wplx = 1019000;
            double Wply = 191000;
            double Jt = 374400;
            double Jw = 313500000000;

            double ASap = 7026.8;
            double JxxSap = 1.592 * 1e8;
            double JyySap = 10413630;
            double WelxMinSap = 874515.5;
            double WelyMinSap = 122513.3;
            double WplxSap = 987756;
            double WplySap = 188932;
            double jtSap = 278151;

            Assert.AreEqual(Math.Abs(A / sec.Area) - 1, 0, 0.05);
            Assert.AreEqual(Math.Abs(ASap / sec.Area) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.05);
            Assert.AreEqual(Math.Abs(JyySap / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(JxxSap / sec.J11) - 1, 0, 0.03);
            Assert.AreEqual(Math.Abs(WelxMin / sec.CalculateWel1()) - 1, 0, 0.05);
            Assert.AreEqual(Math.Abs(WelyMin / sec.CalculateWel2()) - 1, 0, 0.05);
            Assert.AreEqual(Math.Abs(WelxMinSap / sec.CalculateWel1()) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(WelyMinSap / sec.CalculateWel2()) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.012);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.05);
            Assert.AreEqual(Math.Abs(WplySap / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WplxSap / sec.CalculateWpl1()) - 1, 0, 0.015);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.35);
            Assert.AreEqual(jtSap / sec.Jt - 1.0, 0, 0.004);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.025);
        }

        [TestMethod]
        public void SectionHSymmetric_Test6()
        {
            double h = 549;         // sezione da catalogo ArcelorMittal pagina 80/81 UB533x210x138
            double tw = 14.7;
            double bt = 214;
            double bb = 214;
            double tt = 23.6;
            double tb = 23.6;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 210000, 0.3, 355, 510, 7850), string.Empty);

            double A = 17477;          // calcolato con foglio excel marco
            double Jxx = 8.52 * 1e8;
            double Jyy = 3.87 * 1e7;
            double raggioInerziaX = 47.05;
            double raggioInerziaY = 220.83;

            Assert.AreEqual(Math.Abs(A / sec.Area) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(raggioInerziaX / sec.R11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(raggioInerziaY / sec.R22) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionHSymmetric_Test7()
        {
            double h = 290.0;         // HEA300
            double tw = 8.5;
            double bt = 300.0;
            double bb = 300.0;
            double tt = 14.0;
            double tb = 14.0;
            double r = 27.0;

            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 210000, 0.3, 355, 510, 7850), 
                string.Empty, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished, r);

            double A = 11253;          
            double Jxx = 182630000;
            double Jyy = 63100000;
            double raggioInerziaX = 74.9;
            double raggioInerziaY = 127.4;

            Assert.AreEqual(Math.Abs(A / sec.Area) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(raggioInerziaX / sec.R11) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(raggioInerziaY / sec.R22) - 1, 0, 0.005);
        }

        [TestMethod]
        public void SectionHSymmetric_Sigma1()
        {
            double h = 304.8;         
            double tw = 6.35;
            double bt = 127;
            double bb = 127;
            double tt = 9.652;
            double tb = 9.652;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, new SteelMaterial("steel", 210000, 0.3, 355, 510, 7850), string.Empty);

            double sigmaMax = sec.GetMaxSigma(0, 10656502.7, 0);
            double sigmaMin = sec.GetMinSigma(0, 10656502.7, 0);
            double expSigmaMax = 24.71;
            double expSigmaMin = -27.71;

            Assert.IsTrue(Math.Abs(sigmaMax - expSigmaMax) / sigmaMax < 0.001);
            Assert.IsTrue(Math.Abs(sigmaMin - expSigmaMin) / sigmaMin < 0.001);
        }

        [TestMethod]
        public void SectionCHS_Sigma1()
        {
            double d = 300;         
            double t = 8;

            SteelSectionCHS sec = new SteelSectionCHS(d, t, new SteelMaterial("steel", 210000, 0.3, 355, 510, 7850), string.Empty);

            double sigmaMax = sec.GetMaxSigma(0, 11129770.82, 0);
            double sigmaMin = sec.GetMinSigma(0, 11129770.82, 0);
            double expSigmaMax = 21.328;
            double expSigmaMin = -21.328;

            Assert.IsTrue(Math.Abs(sigmaMax - expSigmaMax) / sigmaMax < 0.001);
            Assert.IsTrue(Math.Abs(sigmaMin - expSigmaMin) / sigmaMin < 0.001);
        }

        [TestMethod]
        public void SectionRHS_Sigma1()
        {
            double h = 300;
            double b = 200;
            double t = 8;

            SteelSectionRHS sec = new SteelSectionRHS(h, b, t, t, t, t, new SteelMaterial("steel", 210000, 0.3, 355, 510, 7850), string.Empty);

            double sigmaMax1 = sec.GetMaxSigma(15000, 894116.79, 0);
            double sigmaMin1 = sec.GetMinSigma(15000, 894116.79, 0);
            double expSigmaMax1 = 3.295;
            double expSigmaMin1 = 0.579;

            double sigmaMax2 = sec.GetMaxSigma(6250, 1005881.39, 0);
            double sigmaMin2 = sec.GetMinSigma(6250, 1005881.39, 0);
            double expSigmaMax2 = 2.335;
            double expSigmaMin2 = -0.721;

            Assert.IsTrue(Math.Abs(sigmaMax1 - expSigmaMax1) / sigmaMax1 < 0.001);
            Assert.IsTrue(Math.Abs(sigmaMin1 - expSigmaMin1) / sigmaMin1 < 0.001);
            Assert.IsTrue(Math.Abs(sigmaMax2 - expSigmaMax2) / sigmaMax2 < 0.001);
            Assert.IsTrue(Math.Abs(sigmaMin2 - expSigmaMin2) / sigmaMin2 < 0.001);
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

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.001);
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
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.CalculateWel1()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.CalculateWpl2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.CalculateWpl1()) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.03);
            Assert.AreEqual(JwSAP / sec.Jw - 1, 0, 0.07);
        }

        [TestMethod]
        [TestCategory("Fail: Not implemented Test")]
        public void SectionL_Test1()
        {
            double h = 500;
            double tw = 40;
            double b = 40.01;
            double tb = 40;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double Wel1 = 1.0 / 6.0 * h * tw*tw;
            double Wel2 = 1.0 / 6.0 * tw * h * h;

            Assert.AreEqual(Math.Abs(Wel2 / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel1 / sec.CalculateWel1()) - 1, 0, 0.001);
        }

        [TestMethod]
        [TestCategory("Fail: Not implemented Test")]
        public void SectionL_Test2()
        {
            double h = 40;
            double tw = 40;
            double b = 500;
            double tb = 40;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double Wel1 = 1.0 / 6.0 * b * tb * tb;
            double Wel2 = 1.0 / 6.0 * tb * b * b;

            Assert.AreEqual(Math.Abs(Wel2 / sec.CalculateWel2()) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel1 / sec.CalculateWel1()) - 1, 0, 0.001);
        }

        [TestMethod]
        [TestCategory("Fail: Not implemented Test")]
        public void SectionL_Test3()
        {
            double h = 500;
            double tw = 40;
            double b = 500;
            double tb = 80;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 56800;
            double J2 = 517472668.5153;
            double J1 = 1951689772.799;
            //double Wel1 = 2368295.77;
            //double Wel2 = 5465136.457;

            double JtSAP = 86856533.3;
            double JtStraus = 89173333.33333;
            double Jt = (JtSAP + JtStraus) / 2.0;
            //double JwSAP = 3.696E+11; --> Wrong
            double JwStraus = 1.64361e12;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(J2 / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J1 / sec.J11) - 1, 0, 0.001);
            /*Assert.AreEqual(Math.Abs(Wel2 / sec.Wel22Min) - 1, 0, 0.001); --> SAP ERRATO
            Assert.AreEqual(Math.Abs(Wel1 / sec.Wel11Min) - 1, 0, 0.001); --> SAP ERRATO*/
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.05);
            Assert.AreEqual(JwStraus / sec.Jw - 1, 0, 0.06);
        }

        [TestMethod]
        public void SectionL_Test4()
        {
            double h = 200;
            double tw = 10;
            double b = 200;
            double tb = 10;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 3900;
            double jxx = 15476090;
            double jyy = 15476090;
            double j11 = 24732500;
            double j22 = 6219600;
            double r2 = 79.6;
            double r1 = 39.9;
            double angle = Math.PI / 4;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(angle, sec.AngleX1);
            Assert.AreEqual(Math.Abs(jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(j11 / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(j22 / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(r1 / sec.R11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(r2 / sec.R22) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionL_Test5()
        {
            double h = 200;
            double tw = 10;
            double b = 200;
            double tb = 20;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 5800;
            double jyy = 24551782;
            double jxx = 17407126;
            double j11 = 24732500;
            double j22 = 6219600;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(j11 / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(j22 / sec.J22) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionL_Test6()
        {
            double h = 20.01;
            double tw = 0.1;
            double b = 200;
            double tb = 20;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, new SteelMaterial("steel", 200000, 0.3, 355, 510, 7850), string.Empty);

            double A = 4000.01;
            double jyy = 133334.3;
            double jxx = 13333433;

            Assert.AreEqual(Math.Abs(A/ sec.Area) -1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / sec.Jyy) - 1, 0, 0.001);
        }
    }
}
