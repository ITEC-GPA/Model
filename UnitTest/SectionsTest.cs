using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModelObjectTest
{
    [TestClass]
    public class SectionsTest : UnitTestBase
    {
        /// <summary>
        /// Metodo per visualizzare la geometria della sezione
        /// </summary>
        /// <param name="section"></param>
		private void ExportToGmsh(IConcreteSection section)
        {
            GmshNet.Gmsh.Initialize();

            int[] fillTag = new int[section.Shape.Fill.Count];
            for (int i = 0; i < section.Shape.Fill.Count; i++)
            {
                fillTag[i] = GmshNet.Gmsh.Model.Occ.AddPoint(section.Shape.Fill[i].X, section.Shape.Fill[i].Y, 0.0);
            }

            for (int i = 0; i < fillTag.Length; i++)
            {
                if (i != (fillTag.Length - 1))
                    GmshNet.Gmsh.Model.Occ.AddLine(fillTag[i], fillTag[i + 1]);
                else
                    GmshNet.Gmsh.Model.Occ.AddLine(fillTag[i], fillTag[0]);
            }

            if (section.Shape.HasHoles)
            {
                int[][] holesTag = new int[section.Shape.Holes.Length][];

                for (int i = 0; i < section.Shape.Holes.Length; i++)
                {
                    holesTag[i] = new int[section.Shape.Holes[i].Count];
                    for (int j = 0; j < section.Shape.Holes[i].Count; j++)
                    {
                        holesTag[i][j] = GmshNet.Gmsh.Model.Occ.AddPoint(section.Shape.Holes[i][j].X, section.Shape.Holes[i][j].Y, 0.0);
                    }
                }

                for (int i = 0; i < section.Shape.Holes.Length; i++)
                {
                    for (int j = 0; j < holesTag[i].Length; j++)
                    {
                        if (j != (holesTag[i].Length - 1))
                            GmshNet.Gmsh.Model.Occ.AddLine(holesTag[i][j], holesTag[i][j + 1]);
                        else
                            GmshNet.Gmsh.Model.Occ.AddLine(holesTag[i][j], holesTag[i][0]);
                    }
                }
            }

            foreach (var item in section.Rebars)
            {
                GmshNet.Gmsh.Model.Occ.AddPoint(item.Position.X, item.Position.Y, 0.0);
            }

            GmshNet.Gmsh.Model.Occ.Synchronize();
            GmshNet.Gmsh.Fltk.Run();
            GmshNet.Gmsh.Finalize();
        }

        #region Section CHS

        [TestMethod]
        public void SectionCHS_Test1()
        {
            double d = 400;
            double t = 10;
            double di = d - 2.0 * t;
            SteelSectionCHS sec = new SteelSectionCHS(d, t, new SteelMaterial("S355", 200000, 0.3, 355, 510, 7850), "", Section.FormedTypes.ColdFormed);

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
            Assert.AreEqual(Wel2, sec.Wel1);
            Assert.AreEqual(Wpl2, sec.Wpl2);
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
            double J = 2.89 * 1e7;
            double Wel2 = 2.31 * 1e5;
            double Wpl2 = 3 * 1e5;
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
            Assert.AreEqual(Math.Abs(J / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(J / sec.Jxx) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(J / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jw - sec.Jw), 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / sec.Wpl2) - 1, 0, 0.001);
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
            Assert.AreEqual(Math.Abs(J / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(J / sec.Jxx) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(J / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jw - sec.Jw), 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / sec.Wpl2) - 1, 0, 0.001);
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
            Assert.AreEqual(Math.Abs(J / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(J / sec.Jxx) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(J / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jw - sec.Jw), 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / sec.Wpl2) - 1, 0, 0.001);
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

        #endregion

        #region Section Rectangular

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

            Assert.AreEqual(Math.Abs(A - sec.Area), 0, 0.0015);
            Assert.AreEqual(Math.Abs(J2 - sec.J22), 0, 0.0015);
            Assert.AreEqual(Math.Abs(J1 - sec.J11), 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wel2 - sec.Wel2), 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wel1 - sec.Wel1), 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wpl2 - sec.Wpl2), 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wpl1 - sec.Wpl1), 0, 0.0015);
        }

        #endregion

        #region Section RHS

        [TestMethod]
        public void SectionRHS_Test1()
        {
            double h = 200;
            double b = 100;
            double tf = 20;
            double tw = 10;
            SteelSectionRHS sec = new SteelSectionRHS(h, b, tf, tf, tw, tw, SteelMaterial.S355, string.Empty);

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
            Assert.AreEqual(Math.Abs(Jy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jx / sec.Jxx) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.01);
        }

        [TestMethod]
        public void SectionRHS_Test2()
        {
            double h = 400;
            double b = 200;
            double tf = 10;
            double tw = 10;
            SteelSectionRHS sec = new SteelSectionRHS(h, b, tf, tf, tw, tw, SteelMaterial.S355, string.Empty);

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
            Assert.AreEqual(Math.Abs(Jy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jx / sec.Jxx) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.01);
        }

        [TestMethod]
        public void SectionRHS_Test3()
        {
            double h = 300;
            double b = 300;
            double tf = 15;
            double tw = 15;
            SteelSectionRHS sec = new SteelSectionRHS(h, b, tf, tf, tw, tw, SteelMaterial.S355, string.Empty);

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
            Assert.AreEqual(Math.Abs(Jy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jx / sec.Jxx) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wely / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.01);
        }

        [TestMethod]
        public void SectionRHS_Test4()
        {
            double h = 150;
            double b = 125;
            double tf = 12.5;
            double tw = 12.5;
            SteelSectionRHS sec = new SteelSectionRHS(h, b, tf, tf, tw, tw, SteelMaterial.S355, string.Empty);

            double A = 6250;
            double Jx = 18880208.33;
            double Jy = 13997395.83;

            Assert.AreEqual(A, sec.Area, 0.001);
            Assert.AreEqual(b - 2 * tw, sec.BaseInternal);
            Assert.AreEqual(h - 2 * tf, sec.Heightinternal);
            Assert.AreEqual(Math.Abs(Jy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.J11) - 1, 0, 0.001);
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

        #endregion

        #region Section H

        [TestMethod]
        public void SectionHAsymmetric_Test1()
        {
            double h = 400;
            double tw = 12;
            double bt = 200;
            double bb = 300;
            double tt = 10;
            double tb = 25;
            double radius = 5;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, SteelMaterial.S355, string.Empty, Section.SectionTypes.Welded, Section.FormedTypes.HotFinished, radius);

            double A = 13880 + 4 * Math.Pow((1.41 * radius), 2) / 2.0;
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

            Assert.AreEqual(A, sec.Area, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Wely / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.Wel1) - 1, 0, 0.0121);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.008);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.015);
            Assert.AreEqual(JtSAP / sec.Jt - 1.0, 0, 0.005);
            Assert.AreEqual(JtStraus / sec.Jt - 1.0, 0, 0.05);
            Assert.AreEqual(JwLTBEAM / sec.Jw - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(JtCalc / sec.Jt) - 1, 0, 0.055);
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
            double radius = 5;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, SteelMaterial.S355, string.Empty, Section.SectionTypes.Welded, Section.FormedTypes.HotFinished, radius);

            double A = 20940 + 4 * Math.Pow((1.41 * radius), 2) / 2.0;
            double Jxx = 868212376.6714;
            double Jyy = 167197666.6667;
            double Welx = 2802779;
            double Wely = 835988;
            double Wplx = 3694171;
            double Wply = 1360040;
            double Jt = 2859873;
            double Jw = 6.205 * 1e12;

            Assert.AreEqual(A, sec.Area, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Wely / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.Wel1) - 1, 0, 0.012);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.008);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.015);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.05);
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
            double radius = 5;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, SteelMaterial.S355, string.Empty, Section.SectionTypes.Welded, Section.FormedTypes.HotFinished, radius);

            double A = 28400 + 4 * Math.Pow((1.41 * radius), 2) / 2.0;
            double Jxx = 828928638;
            double Jyy = 439238667;
            double WelxMin = 2162952;
            double WelyMin = 1756955;
            double Wplx = 2879887;
            double Wplxsap = 2912720;
            double Wply = 2741200;
            double Jt = 11040267;
            double jtSap = 10481812;
            double jtStraus = 10534103.36714;
            double Jw = 4816472960152;

            Assert.AreEqual(A, sec.Area, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(WelyMin / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.Wel1) - 1, 0, 0.012);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.008);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.018);
            Assert.AreEqual(Math.Abs(Wplxsap / sec.Wpl1) - 1, 0, 0.007);
            Assert.AreEqual(jtStraus / sec.Jt - 1.0, 0, 0.001);
            Assert.AreEqual(jtSap / sec.Jt - 1.0, 0, 0.005);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.05);
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
            double radius = 5;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, SteelMaterial.S355, string.Empty, Section.SectionTypes.Welded, Section.FormedTypes.HotFinished, radius);

            double A = 28400 + 4 * Math.Pow((1.41 * radius), 2) / 2.0;
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

            Assert.AreEqual(A, sec.Area, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(WelyMin / sec.Wel2) - 1, 0, 0.01);
            Assert.AreEqual(Math.Abs(WelxMin / sec.Wel1) - 1, 0, 0.012);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.008);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.018);
            Assert.AreEqual(Math.Abs(WplxSap / sec.Wpl1) - 1, 0, 0.007);
            Assert.AreEqual(jtStraus / sec.Jt - 1.0, 0, 0.001);
            Assert.AreEqual(jtSap / sec.Jt - 1.0, 0, 0.005);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.05);
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
            double radius = 5;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, SteelMaterial.S355, string.Empty, Section.SectionTypes.Welded, Section.FormedTypes.HotFinished, radius);

            double A = 28640 + 4 * Math.Pow((1.41 * radius), 2) / 2.0;
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

            Assert.AreEqual(A, sec.Area, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(WelyMin / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.Wel1) - 1, 0, 0.012);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.008);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.0185);
            Assert.AreEqual(Math.Abs(WplxSap / sec.Wpl1) - 1, 0, 0.0065);
            Assert.AreEqual(JtSap / sec.Jt - 1.0, 0, 0.005);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(JtSSRC / sec.Jt) - 1, 0, 0.05);
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
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, SteelMaterial.S355, string.Empty);

            double A = 25600;
            double Jxx = 761333333.3333;
            double Jyy = 266803200.0000;
            double WelxMin = 3806667;
            double WelyMin = 1334016;
            double Wplx = 4240000;
            double Wply = 2022400;
            double Jt = 4678667;
            double Jw = 9.375 * 1e12;

            Assert.AreEqual(A, sec.Area, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.001);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.05);
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
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, SteelMaterial.S355, string.Empty);

            double A = 23800;
            double Jxx = 1094333333.3333;
            double Jyy = 178760166.6667;
            double WelxMin = 4377333;
            double WelyMin = 1021487;
            double Wplx = 4865000;
            double Wply = 1553300;
            double Jt = 4080300;
            double Jw = 1.008 * 1e13;

            Assert.AreEqual(A, sec.Area, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelxMin / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.05);
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
            double r = 15;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, SteelMaterial.S355, string.Empty,
                Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished, r);

            double A = 165470;
            double Jxx = 7549500000;
            double Jyy = 2544100000;
            double WelxMin = 25160000;
            double WelyMin = 10680000;
            double Wplx = 33260000;
            double Wply = 16670000;
            double jtSap = 7.95 * 1e8;
            double Jt = 944000000;  // da catalogo
            double Jw = 133120000000000;

            Assert.AreEqual(Math.Abs(A / sec.Area) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(WelxMin / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(WelyMin / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.0015);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.005);
            Assert.AreEqual(jtSap / sec.Jt - 1.0, 0, 0.17);
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
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, SteelMaterial.S355, string.Empty);

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
            Assert.AreEqual(Math.Abs(WelxMin / sec.Wel1) - 1, 0, 0.0035);
            Assert.AreEqual(Math.Abs(WelyMin / sec.Wel2) - 1, 0, 0.0035);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.0011);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.0035);
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
            double radius = 18;
            SteelSectionH sec = new SteelSectionH(h, tw, bt, tt, bb, tb, SteelMaterial.S355,
                string.Empty, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished, radius);

            double A = 7270;          // da catalogo (tiene in conto anche i raggi)
            double Jxx = 162600000;
            double Jyy = 10430000;
            double WelxMin = 903600;
            double WelyMin = 122700;
            double Wplx = 1019000;
            double Wply = 191000;
            double Jt = 374400;
            double Jw = 313500000000;
            // non considera i raggi
            double ASap = 7026.8;
            double JxxSap = 1.592 * 1e8;
            double JyySap = 10413630;
            double WelxMinSap = 874515.5;
            double WelyMinSap = 122513.3;
            double WplxSap = 987756;
            double WplySap = 188932;
            double jtSap = 278151;

            Assert.AreEqual(Math.Abs(A / sec.Area) - 1, 0, 0.05);
            Assert.AreEqual(Math.Abs(ASap / sec.Area) - 1, 0, 0.035);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.05);
            Assert.AreEqual(Math.Abs(JyySap / sec.J22) - 1, 0, 0.006);
            Assert.AreEqual(Math.Abs(JxxSap / sec.J11) - 1, 0, 0.03);
            Assert.AreEqual(Math.Abs(WelxMin / sec.Wel1) - 1, 0, 0.01);
            Assert.AreEqual(Math.Abs(WelyMin / sec.Wel2) - 1, 0, 0.01);
            Assert.AreEqual(Math.Abs(WelxMinSap / sec.Wel1) - 1, 0, 0.034);
            Assert.AreEqual(Math.Abs(WelyMinSap / sec.Wel2) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.028);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.01);
            Assert.AreEqual(Math.Abs(WplySap / sec.Wpl2) - 1, 0, 0.038);
            Assert.AreEqual(Math.Abs(WplxSap / sec.Wpl1) - 1, 0, 0.024);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.35);
            Assert.AreEqual(Math.Abs(jtSap / sec.Jt - 1.0), 0, 0.3);
            Assert.AreEqual(Math.Abs(Jw / sec.Jw) - 1, 0, 0.025);
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
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.0015);
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
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(raggioInerziaX / sec.R11) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(raggioInerziaY / sec.R22) - 1, 0, 0.005);
        }

        [TestMethod]
        public void SectionHSymmetric_Test8()
        {
            double h = 300.0;
            double width = 150.0;
            double flangeThickness = 10.7;
            double webThickness = 7.1;
            double r = 15.0;

            SteelSectionH sec = new SteelSectionH(h, webThickness, width, flangeThickness, width, flangeThickness, new SteelMaterial("steel", 210000, 0.3, 355, 510, 7850),
                string.Empty, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished, r);

            double Jxx = 83560000;
            double Jyy = 6038000;
            double Jt = 201000;
            double Jw = 125934100000;
            double Wel1 = 557100;
            double Wpl1 = 628400;

            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1, 0, 0.01);
            Assert.AreEqual(Math.Abs(Jw / sec.Jw) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Wel1 / sec.Wel1) - 1, 0, 0.005);
            Assert.AreEqual(Math.Abs(Wpl1 / sec.Wpl1) - 1, 0, 0.0075);
        }

        [TestMethod]
        public void SectionHSymmetric_OutPutPoints()
        {
            double h = 300.0;
            double width = 150.0;
            double flangeThickness = 20;
            double webThickness = 10;
            double r = 15.0;

            SteelSectionH sec = new SteelSectionH(h, webThickness, width, flangeThickness, width, flangeThickness, new SteelMaterial("steel", 210000, 0.3, 355, 510, 7850),
                string.Empty, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished, r);

            Point2d[] points = sec.GetSectionPoints();
            List<Point2d> list = new List<Point2d>();
            double item = 1;

            foreach (Point2d p in points)
            {
                list.Add(p);
                Console.WriteLine($"Point {item}: {p.X}, {p.Y}");
                item++;
            }

            List<Point2d> expPoints = new List<Point2d>()
            {
                new Point2d(0,0),
                new Point2d(150,0),
                new Point2d(150,20),
                new Point2d(0,20),
                new Point2d(0,300),
                new Point2d(150,300),
                new Point2d(150,280),
                new Point2d(0,280),
                new Point2d(70,280),
                new Point2d(80,280),
                new Point2d(70,20),
                new Point2d(80,20),
            };

            foreach (Point2d pt in expPoints)
                Assert.IsTrue(list.Contains(pt));
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

        #endregion

        #region Section T

        [TestMethod]
        public void SectionT_Test()
        {
            double h = 400;
            double b = 200;
            double tf = 10;
            double tw = 50;
            SectionT sec = new SectionT(h, b, tw, tf, SteelMaterial.S355, string.Empty);

            double A = 21500;
            double Jxx = 3.197 * 1e8;
            double Jyy = 10729167;
            double Welx = 1496864.9;
            double Wely = 107291.67;
            double Wplx = 2281250;
            double Wply = 343750;
            double Jt = 16316666.66667; //Straus : 16316666.66667 | Sap: 15845817

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wely / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / sec.Jt) - 1.0, 0, 0.03);
        }

        [TestMethod]
        public void SectionT_Test2()
        {
            double h = 400;
            double b = 200;
            double tf = 10;
            double tw = 20;
            SectionT sec = new SectionT(h, b, tw, tf, SteelMaterial.S355, string.Empty);

            //double JwLTBEAM = 39044 * 1e6; // --> WRONG
            double JwStraus = 1.32284 * 1e10;
            //double JwSAP = 13751083333;
            Assert.AreEqual(JwStraus / sec.Jw - 1.0, 0, 0.05);
        }

        #endregion

        #region Section C

        [TestMethod]
        public void SectionC_Test1()
        {
            double h = 400;
            double tw = 10;
            double b = 200;
            double tf = 25;
            SectionC sec = new SteelSectionC(h, tw, b, tf, b, tf, SteelMaterial.S355, string.Empty);

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
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wely / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wply / sec.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.001);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.03);
            Assert.AreEqual(JwSAP / sec.Jw - 1, 0, 0.07);
        }

        [TestMethod]
        public void SectionC_Test2()
        {
            // UPN 300 ArcelorMittal
            double h = 300.0;
            double width = 100.0;
            double flangeThickness = 16.0;
            double webThickness = 10.0;
            double radius1 = 16.0;
            double radius2 = 8.0;
            SteelSectionC sec = new SteelSectionC(h, webThickness, width, flangeThickness, width, flangeThickness, SteelMaterial.S355,
                string.Empty, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished, radius1, radius2);

            double A = 5880;
            double Jyy = 4950000;   // noi non consideriamo l'inclinazione
            double Jxx = 80300000;
            double Welx = 535000;
            double Wplx = 632000;
            double Jt = 374000;
            double Jw = 69100000000;

            Assert.AreEqual(Math.Abs(A / sec.Area - 1), 0, 0.02);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22 - 1), 0, 0.11);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11 - 1), 0, 0.03);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.11);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.03);
            Assert.AreEqual(Math.Abs(Welx / sec.Wel1) - 1, 0, 0.035);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.02);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.057);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.14);
        }

        #endregion

        #region Section L

        [TestMethod]
        public void SectionL_Test1()
        {
            double h = 250;
            double tw = 25;
            double b = 250;
            double tb = 15;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, SteelMaterial.S355, string.Empty);

            double A = 9625;
            double jxx = 62872568;
            double jyy = 48806903;
            double Wel1 = 378291.7;
            double Wel2 = 252012.17;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel1 / sec.WelX) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Wel2 / sec.WelY) - 1, 0, 0.015);
        }

        [TestMethod]
        public void SectionL_Test2()
        {
            double h = 40;
            double tw = 40;
            double b = 500;
            double tb = 40;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, SteelMaterial.S355, string.Empty);

            double Wel1 = 1.0 / 6.0 * b * tb * tb;
            double Wel2 = 1.0 / 6.0 * tb * b * b;

            Assert.AreEqual(Math.Abs(Wel2 / sec.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel1 / sec.Wel1) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionL_Test3()
        {
            double h = 500;
            double tw = 40;
            double b = 500;
            double tb = 80;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, SteelMaterial.S355, string.Empty);

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
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, SteelMaterial.S355, string.Empty);

            double A = 3900;
            double jxx = 15476090;
            double jyy = 15476090;
            double j11 = 24732500;
            double j22 = 6219600;
            double r2 = 79.6;
            double r1 = 39.9;
            double angle = Math.PI / 4;
            double Wpl1 = 106992.1926364234;
            double Wpl2 = 106992.1926364234;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(angle, sec.AngleX1);
            Assert.AreEqual(Math.Abs(jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(j11 / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(j22 / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(r1 / sec.R11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(r2 / sec.R22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl1 / sec.WelX) - 1, 0, 0.015);
            Assert.AreEqual(Math.Abs(Wpl2 / sec.WelY) - 1, 0, 0.015);
        }

        [TestMethod]
        public void SectionL_Test5()
        {
            double h = 200;
            double tw = 10;
            double b = 200;
            double tb = 20;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, SteelMaterial.S355, string.Empty);

            double A = 5800;
            double jyy = 24551782;
            double jxx = 17407126;
            double j11 = 33301743.312114567;
            double j22 = 8657164.733862447;
            double angle = 36.57378 * Math.PI / 180.0;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(j11 / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(j22 / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(angle / sec.AngleX1) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionL_Test6()
        {
            double h = 20.1;
            double tw = 0.1;
            double b = 200;
            double tb = 20;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, SteelMaterial.S355, string.Empty);

            double A = 4000.01;
            double jxx = 133334.34;
            double jyy = 13333433.23;

            Assert.AreEqual(Math.Abs(A / sec.Area) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / sec.Jyy) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionL_Test7()
        {
            double h = 500;
            double tw = 40;
            double b = 500;
            double tb = 40;
            SteelSectionL sec = new SteelSectionL(b, tb, h, tw, SteelMaterial.S355, string.Empty);

            double A = 38400;
            double J2 = 375037668.5153;
            double J1 = 1477129772.799;
            double teta = Math.PI / 4.0;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(J2 / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J1 / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(teta / sec.AngleX1) - 1, 0, 0.001);
        }

        #endregion

        #region Section Rectangular

        [TestMethod]
        public void SectionRectangularTest1()
        {
            double h = 500;
            double b = 300;
            ConcreteSectionRectangular section = new ConcreteSectionRectangular(h, b, ConcreteMaterialEN1992.C40_50, "Section");

            double A = 150000;
            double jxx = 3.125 * 1e9;
            double jyy = 1.125 * 1e9;
            double Wel1 = 12500000;
            double Wel2 = 7500000;
            double Wpl1 = 18750000;
            double Wpl2 = 11250000;
            double Jt = 2.817 * 1e9;

            Assert.AreEqual(Math.Abs(A / section.Area) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jxx / section.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / section.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel1 / section.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / section.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl1 / section.Wpl1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / section.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / section.Jt) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionRectangularTest2()
        {
            double h = 600;
            double b = 350;
            ConcreteSectionRectangular section = new ConcreteSectionRectangular(h, b, ConcreteMaterialEN1992.C40_50, "Section");

            double A = 210000;
            double jxx = 6.300 * 1e9;
            double jyy = 2.144 * 1e9;
            double Wel1 = 21000000;
            double Wel2 = 12250000;
            double Wpl1 = 31500000;
            double Wpl2 = 18375000;
            double Jt = 5.45 * 1e9;

            Assert.AreEqual(Math.Abs(A / section.Area) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jxx / section.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / section.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / section.J22) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(jxx / section.J11) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wel1 / section.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / section.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl1 / section.Wpl1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / section.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / section.Jt) - 1, 0, 0.001);
        }

        #endregion

        #region Section Circular

        [TestMethod]
        public void SectionCircularSubdivision2()
        {
            double d = 500;
            ConcreteSectionCircular section = new ConcreteSectionCircular(d, ConcreteMaterialEN1992.C40_50, "Section");

            double A = 196349.54;
            double jxx = 3.068 * 1e9;
            double jyy = 3.068 * 1e9;
            double Wel1 = 12271846;
            double Wel2 = 12271846;
            double Wpl1 = 20833333;
            double Wpl2 = 20833333;
            double Jt = 6.136 * 1e9;

            Assert.AreEqual(Math.Abs(A / section.Area) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jxx / section.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / section.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / section.J22) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(jxx / section.J11) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wel1 / section.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / section.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl1 / section.Wpl1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / section.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / section.Jt) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionCircularTest2()
        {
            double d = 400;
            ConcreteSectionCircular section = new ConcreteSectionCircular(d, ConcreteMaterialEN1992.C40_50, "Section");

            double A = 125663.71;
            double jxx = 1.257 * 1e9;
            double jyy = 1.257 * 1e9;
            double Wel1 = 6283185;
            double Wel2 = 6283185;
            double Wpl1 = 10666666;
            double Wpl2 = 10666666;
            double Jt = 2.513 * 1e9;

            Assert.AreEqual(Math.Abs(A / section.Area) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jxx / section.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / section.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(jyy / section.J22) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(jxx / section.J11) - 1, 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wel1 / section.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wel2 / section.Wel2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl1 / section.Wpl1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wpl2 / section.Wpl2) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jt / section.Jt) - 1, 0, 0.001);
        }

        #endregion

        #region Section Generic

        [TestMethod]
        public void SectionGenericTest1()
        {
            //// Section
            ////      ____________
            ////     /            \
            ////    /              \
            ////   /                \
            ////  /                  \
            //// /____________________\
            //// 
            //// 4 points
            //double height = 400;
            //double baseMaj = 400;
            //double[] thickness = new double[] { 20, 10, 20, 10 };

            //Point2d[] vertices = new Point2d[]
            //{
            //    new Point2d(0,0),
            //    new Point2d(baseMaj,0),
            //    new Point2d(baseMaj,height),
            //    new Point2d(0,height)
            //};

            //Line2d[] lines = new Line2d[]
            //{
            //    new Line2d(vertices[0], vertices[1]),
            //    new Line2d(vertices[1], vertices[2]),
            //    new Line2d(vertices[2], vertices[3]),
            //    new Line2d(vertices[3], vertices[0])
            //};

            //SteelSectionGeneric sectionGeneric = new SteelSectionGeneric(lines, thickness, SteelMaterial.S355, "");

            //SteelSectionRHS steelSectionRHS = new SteelSectionRHS(height + thickness[0] / 2 + thickness[2] / 2, 
            //    baseMaj + thickness[1] / 2 + thickness[3] / 2, 
            //    thickness[2], thickness[0], thickness[3], thickness[1], SteelMaterial.S355, "");

            //Assert.IsTrue(Math.Abs(sectionGeneric.Area - steelSectionRHS.Area) / steelSectionRHS.Area * 100 < 1);
            //Assert.IsTrue(Math.Abs(sectionGeneric.J11 - steelSectionRHS.J11) / steelSectionRHS.J11 * 100 < 1);
            //Assert.IsTrue(Math.Abs(sectionGeneric.J22 - steelSectionRHS.J22) / steelSectionRHS.J22 * 100 < 1);
            //Assert.IsTrue(Math.Abs(sectionGeneric.AngleX1 - steelSectionRHS.AngleX1) < 1);
        }

        #endregion

        #region Concrete Section

        [TestMethod]
        public void RCRectangularSection1()
        {
            double heigth = 500;
            double width = 300;
            double rebarDiameter = 18;
            double n = 15;

            // sezione rettangolare 300x500
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[] {   new Point2d(0, 0),
                                                                    new Point2d(width, 0),
                                                                    new Point2d(width, heigth),
                                                                    new Point2d(0, heigth) }));

            ShapeEx shapeEx = new ShapeEx(shape, ConcreteMaterialEN1992.C25_30);
            GPC.Model.Sections.Rebar.RebarSectionCircular rebar = new GPC.Model.Sections.Rebar.RebarSectionCircular(rebarDiameter, RebarMaterial.B450C);

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] {  new ReinforcedConcreteRebar(rebar, new Point2d(50,50)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(250, 50)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(250,450)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(50,450))};
            ReinforcedConcreteSection section = new ReinforcedConcreteSection(shapeEx);
            section.AddRebars(rebars);

            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(n);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(section.Area - 150000) / section.Area * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jxx - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J11 - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J22 - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.AngleX1) < 0.001);
            Assert.IsTrue(Math.Abs(mechanicalProperties.areaH - 165240) / mechanicalProperties.areaH * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J11H - 3693960000) / mechanicalProperties.J11H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J22H - 1267240000) / mechanicalProperties.J22H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.angleX) < 0.001);

            ConcreteSectionRectangular sectionRectangular = new ConcreteSectionRectangular(heigth, width, ConcreteMaterialEN1992.C25_30);
            sectionRectangular.AddRebars(rebars);

            mechanicalProperties = section.GetHomogeneizedMechanicalProperties(n);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(sectionRectangular.Area - 150000) / section.Area * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jxx - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(sectionRectangular.J11 - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(sectionRectangular.J22 - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(sectionRectangular.AngleX1) < 0.001);
            Assert.IsTrue(Math.Abs(mechanicalProperties.areaH - 165240) / mechanicalProperties.areaH * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J11H - 3693960000) / mechanicalProperties.J11H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J22H - 1267240000) / mechanicalProperties.J22H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.angleX) < 0.001);
        }

        [TestMethod]
        public void RCRectangularSection2()
        {
            double heigth = 500;
            double width = 300;
            double rebarDiameter = 18;
            double n = 15;

            // sezione rettangolare 300x500
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[] {   new Point2d(0, 0),
                                                                    new Point2d(width, 0),
                                                                    new Point2d(width, heigth),
                                                                    new Point2d(0, heigth), }));

            ShapeEx shapeEx = new ShapeEx(shape, ConcreteMaterialEN1992.C25_30);
            GPC.Model.Sections.Rebar.RebarSectionCircular rebar = new GPC.Model.Sections.Rebar.RebarSectionCircular(rebarDiameter, RebarMaterial.B450C);

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] {  new ReinforcedConcreteRebar(rebar, new Point3d(50,50,0)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point3d(100, 50, 0)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point3d(200, 50,0)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point3d(250, 50,0))};

            ReinforcedConcreteSection section = new ReinforcedConcreteSection(shapeEx);
            section.AddRebars(rebars);
            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(n);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(section.J11 - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J22 - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jxx - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J11H - 3644680436) / mechanicalProperties.J11H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J22H - 1213900000) / mechanicalProperties.J22H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.angleX) < 0.001);

            ConcreteSectionRectangular sectionRectangular = new ConcreteSectionRectangular(heigth, width, ConcreteMaterialEN1992.C25_30);
            sectionRectangular.AddRebars(rebars);

            mechanicalProperties = sectionRectangular.GetHomogeneizedMechanicalProperties(n);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(section.J11 - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J22 - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jxx - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J11H - 3644680436) / mechanicalProperties.J11H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J22H - 1213900000) / mechanicalProperties.J22H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.angleX) < 0.001);

        }

        [TestMethod]
        public void RCRectangularSection3()
        {
            double heigth = 500;
            double width = 300;
            double rebarDiameter = 16;
            double n = 15;

            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[] {   new Point2d(0, 0),
                                                                        new Point2d(width, 0),
                                                                        new Point2d(width, heigth),
                                                                        new Point2d(0, heigth), }));

            ShapeEx shapeEx = new ShapeEx(shape, ConcreteMaterialEN1992.C25_30);
            GPC.Model.Sections.Rebar.RebarSectionCircular rebar = new GPC.Model.Sections.Rebar.RebarSectionCircular(rebarDiameter, RebarMaterial.B450C);

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] {  new ReinforcedConcreteRebar(rebar, new Point3d(50,50,0)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point3d(100, 50, 0)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point3d(200, 50,0)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point3d(250, 50,0)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point3d(50,450,0)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point3d(100, 450, 0)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point3d(200, 450,0)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point3d(250, 450,0))};

            ReinforcedConcreteSection section = new ReinforcedConcreteSection(shapeEx);
            section.AddRebars(rebars);
            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(n);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(mechanicalProperties.J11H - 4025480000) / mechanicalProperties.J11H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J22H - 1265700000) / mechanicalProperties.J22H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.angleX) < 0.001);




            ConcreteSectionRectangular sectionRectangular = new ConcreteSectionRectangular(heigth, width, ConcreteMaterialEN1992.C25_30);
            sectionRectangular.AddRebars(rebars);

            mechanicalProperties = sectionRectangular.GetHomogeneizedMechanicalProperties(n);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(mechanicalProperties.J11H - 4025480000) / mechanicalProperties.J11H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J22H - 1265700000) / mechanicalProperties.J22H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.angleX) < 0.001);
        }

        [TestMethod]
        public void RCGenericSection1()
        {
            // sezion generica a 4 punti
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[] {   new Point2d(0, 0),
                                                                        new Point2d(500, 100),
                                                                        new Point2d(400, 300),
                                                                        new Point2d(100, 200) }));

            ShapeEx shapeEx = new ShapeEx(shape, ConcreteMaterialEN1992.C25_30);
            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] { };

            ReinforcedConcreteSection section = new ReinforcedConcreteSection(shapeEx);
            section.AddRebars(rebars);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(section.Jxx - 330208333) / section.Jxx * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 1127777778) / section.Jyy * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J11 - 1211051091) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J22 - 246935021) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.AngleX1 - (-0.29827677)) < 0.001);
        }

        [TestMethod]
        public void RCTSection1()
        {
            // sezion a T tovescia 
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[] {   new Point2d(0, 0),
                                                                        new Point2d(500, 0),
                                                                        new Point2d(500, 500),
                                                                        new Point2d(400, 500),
                                                                        new Point2d(400, 1000),
                                                                        new Point2d(100, 1000),
                                                                        new Point2d(100, 500),
                                                                        new Point2d(0, 500) }));

            ShapeEx shapeEx = new ShapeEx(shape, ConcreteMaterialEN1992.C25_30);
            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] { };

            ReinforcedConcreteSection section = new ReinforcedConcreteSection(shapeEx);
            section.AddRebars(rebars);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(section.Jxx - 31770833333) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 6333333333) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J11 - 31770833333) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J22 - 6333333333) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.AngleX1) < 0.001);
        }

        [TestMethod]
        public void RCCircularSection1()
        {
            double rebarDiameter = 16;
            double diameter = 500;
            double n = 16;

            GPC.Model.Sections.Rebar.RebarSectionCircular rebar = new GPC.Model.Sections.Rebar.RebarSectionCircular(rebarDiameter, RebarMaterial.B450C);

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] {  new ReinforcedConcreteRebar(rebar, new Point2d(450, 250), 0),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(434.77591, 326.536678)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(391.421355, 391.421357)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(326.536686, 434.775907)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(250, 450)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(173.463322, 434.77591)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(108.578643, 391.421355)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(65.224093, 326.536686)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(50, 250)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(65.22409, 173.463322)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(108.578645, 108.578643)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(173.463314, 65.224093)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(250.0, 50)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(326.536678, 65.22409)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(391.421357, 108.578645)),
                                                                                new ReinforcedConcreteRebar(rebar, new Point2d(434.775907, 173.463314)) };

            ConcreteSectionCircular section = new ConcreteSectionCircular(diameter, ConcreteMaterialEN1992.C25_30);
            section.AddRebars(rebars);

            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(n);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(mechanicalProperties.J11H - 4018160897) / mechanicalProperties.J11H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J22H - 4018160897) / mechanicalProperties.J22H * 100 < 1);

            section = new ConcreteSectionCircular(diameter, ConcreteMaterialEN1992.C25_30);
           
            section.AddRadialRebars(50, rebars.Length, rebar);


            mechanicalProperties = section.GetHomogeneizedMechanicalProperties(n);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(mechanicalProperties.J11H - 4018160897) / mechanicalProperties.J11H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J22H - 4018160897) / mechanicalProperties.J22H * 100 < 1);
        }

        [TestMethod]
        public void RCCHSSection1()
        {
            double rebarDiameter = 16;
            double diameterExternal = 500;
            double thickness = 100;
            double concreteCover = 50;
            int numberOfRebars = 16;
            double n = 16;

            GPC.Model.Sections.Rebar.RebarSectionCircular rebar = new GPC.Model.Sections.Rebar.RebarSectionCircular(rebarDiameter, RebarMaterial.B450C);

            ConcreteSectionCHS section = new ConcreteSectionCHS(diameterExternal, thickness, ConcreteMaterialEN1992.C25_30);
            section.AddRadialRebars(concreteCover, numberOfRebars, rebar);

            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(n);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(mechanicalProperties.J11H - 3622483885) / mechanicalProperties.J11H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J22H - 3622483885) / mechanicalProperties.J22H * 100 < 1);
        }

        [TestMethod]
        public void RCCHSSection2()
        {
            double rebarDiameter = 26;
            double diameterExternal = 1000;
            double thickness = 100;
            double concreteCover = 50;
            int numberOfRebars = 32;
            double n = 16;

            GPC.Model.Sections.Rebar.RebarSectionCircular rebar = new GPC.Model.Sections.Rebar.RebarSectionCircular(rebarDiameter, RebarMaterial.B450C);

            ConcreteSectionCHS section = new ConcreteSectionCHS(diameterExternal, thickness, ConcreteMaterialEN1992.C25_30);
            section.AddRadialRebars(concreteCover, numberOfRebars, rebar);

            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(n);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(mechanicalProperties.J11H - 54653616364) / mechanicalProperties.J11H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J22H - 54653616364) / mechanicalProperties.J22H * 100 < 1);
        }

        #endregion
    }
}