using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Maths.GaussIntegrations;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Sections.Steel;
using GPC.TestUtilities;
using GPC.Utilities.Extensions;
using GPC.Utilities.Maths;
using MathNet.Numerics.Distributions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Metodo per visualizzare la mesh 
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        protected Point3d[] ExportToGmsh(Mesh mesh)
        {
            GmshNet.Gmsh.Initialize();
            List<Point3d> points = new List<Point3d>();

            for (int i = 1; i <= mesh.FacesCount; i++)
            {
                MeshVertex[] vertices = mesh.GetFaceVertices(mesh.Faces[i]);
                List<int> indicesV = new List<int>();
                List<int> indicesL = new List<int>();

                for (int k = 0; k < vertices.Length; k++)
                {
                    try
                    {
                        indicesV.Add(GmshNet.Gmsh.Model.Occ.AddPoint(vertices[k].Point.X / 1000000,
                            vertices[k].Point.Y / 1000000,
                            vertices[k].Point.Z / 10000));
                    }
                    catch { }
                }

                for (int k = 0; k < indicesV.Count; k++)
                {
                    try
                    {
                        if (k != indicesV.Count - 1)
                            indicesL.Add(GmshNet.Gmsh.Model.Occ.AddLine(indicesV[k], indicesV[k + 1]));
                        else
                            indicesL.Add(GmshNet.Gmsh.Model.Occ.AddLine(indicesV[k], indicesV[0]));
                    }
                    catch { }
                }

                try
                {
                    int wire = GmshNet.Gmsh.Model.Occ.AddWire(indicesL.ToArray());
                    GmshNet.Gmsh.Model.Occ.AddPlaneSurface(new int[] { wire });
                }
                catch { }
            }

            GmshNet.Gmsh.Model.Occ.Synchronize();

            GmshNet.Gmsh.Model.Mesh.Generate(0);
            GmshNet.Gmsh.Model.Mesh.Generate(1);
            //GmshNet.Gmsh.Model.Mesh.Generate(2);

            GmshNet.Gmsh.Model.Occ.Synchronize();

            GmshNet.Gmsh.Fltk.Run();
            GmshNet.Gmsh.Finalize();

            return points.ToArray();
        }

        protected Shape2d GetRectangularShape2d(double width, double heigth)
        {
            return new Shape2d(new Polygon2d(new Point2d[] {
                new Point2d(0, 0),
                new Point2d(width, 0),
                new Point2d(width, heigth),
                new Point2d(0, heigth) }));
        }

        #region Section CHS

        [TestMethod]
        public void SectionCHS_Test1()
        {
            double d = 400;
            double t = 10;
            double di = d - 2.0 * t;
            var sec = new SteelSection(new SectionCHS(d, t, ""), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.ColdFormed);

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
            var sec = new SteelSection(new SectionCHS(d, t, ""), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.ColdFormed);

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
            var sec = new SteelSection(new SectionCHS(d, t, ""), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.ColdFormed);

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
            var sec = new SteelSection(new SectionCHS(d, t, ""), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.ColdFormed);

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
        public void SectionCHS_Test5()
        {
            double d = 400;
            double t = 10;
            double di = d - 2.0 * t;
            var sec = new SteelSection(new SectionCHS(d, t, ""), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.ColdFormed);

            Point2d centroid = new Point2d(d / 2, d / 2);
            double A = Math.PI * (d * d - di * di) / 4.0;
            double J = Math.PI * (Math.Pow(d, 4) - Math.Pow(di, 4)) / 64.0;
            double Wel2 = J / (d / 2.0);
            double Wpl2 = (Math.Pow(d, 3.0) - Math.Pow(di, 3.0)) / 6.0;

            // Rigidity factor.
            double flexModule = 2.0 / d;

            SectionPropertiesIntegrals(sec, flexModule, centroid,
                out double JxxIntegral, out double JyyIntegral, out double JxyIntegral, out double Wel1Integral, out double Wpl1Integral);

            // Error without fillet radius (radius=0.0).
            Assert.AreEqual(0.0, JxxIntegral / sec.J11 - 1.0, 0.01);
            Assert.AreEqual(0.0, JyyIntegral / sec.J22 - 1.0, 0.01);
            Assert.AreEqual(sec.Jxy, JxyIntegral, 1);
            Assert.AreEqual(0.0, Wel1Integral / sec.Wel1 - 1.0, 0.01);
            Assert.AreEqual(0.0, Wpl1Integral / sec.Wpl1 - 1.0, 0.01);
        }

        [TestMethod]
        public void SectionCHS_Sigma1()
        {
            double d = 300;
            double t = 8;

            var sec = new SteelSection(new SectionCHS(d, t, string.Empty), SteelMaterialEN1993Data.S355);

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
        public void SectionRectangularTest01()
        {
            double h = 100;
            double b = 10;
            SectionRectangular sec = new SectionRectangular(b, h);

            double A = h * b;
            double J1 = 1.0 / 12.0 * b * Math.Pow(h, 3.0);
            double J2 = 1.0 / 12.0 * h * Math.Pow(b, 3.0);
            double Jxx = J2;
            double Jyy = J1;
            double Jxy = 0.0;
            double Wel2 = 1.0 / 6.0 * b * Math.Pow(h, 2.0);
            double Wel1 = 1.0 / 6.0 * h * Math.Pow(b, 2.0);
            double Wpl2 = A / 2.0 * h / 2.0;
            double Wpl1 = A / 2.0 * b / 2.0;

            Assert.AreEqual(Math.Abs(A - sec.Area), 0, 0.0015);
            Assert.AreEqual(Math.Abs(J2 - sec.J22), 0, 0.0015);
            Assert.AreEqual(Math.Abs(J1 - sec.J11), 0, 0.0015);
            Assert.AreEqual(0, Math.Abs(Jxx - sec.Jxx), 0.0015);
            Assert.AreEqual(0, Math.Abs(Jyy - sec.Jyy), 0.0015);
            Assert.AreEqual(0, Math.Abs(Jxy - sec.Jxy), 0.0015);
            Assert.AreEqual(Math.Abs(Wel2 - sec.Wel2), 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wel1 - sec.Wel1), 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wpl2 - sec.Wpl2), 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wpl1 - sec.Wpl1), 0, 0.0015);
        }

        [TestMethod]
        public void SectionRectangularTest02()
        {
            double h = 2.0;
            double b = 10.0;
            SectionRectangular sec = new SectionRectangular(h, b);

            double A = h * b;
            double J2 = 1.0 / 12.0 * b * Math.Pow(h, 3.0);
            double J1 = 1.0 / 12.0 * h * Math.Pow(b, 3.0);
            double Jxx = J2;
            double Jyy = J1;
            double Jxy = 0.0;
            double Wel1 = 1.0 / 6.0 * b * Math.Pow(h, 2.0);
            double Wel2 = 1.0 / 6.0 * h * Math.Pow(b, 2.0);
            double Wpl1 = A / 2.0 * h / 2.0;
            double Wpl2 = A / 2.0 * b / 2.0;

            Assert.AreEqual(Math.Abs(A - sec.Area), 0, 0.0015);
            Assert.AreEqual(Math.Abs(J2 - sec.J22), 0, 0.0015);
            Assert.AreEqual(Math.Abs(J1 - sec.J11), 0, 0.0015);
            Assert.AreEqual(0, Math.Abs(Jxx - sec.Jxx), 0.0015);
            Assert.AreEqual(0, Math.Abs(Jyy - sec.Jyy), 0.0015);
            Assert.AreEqual(0, Math.Abs(Jxy - sec.Jxy), 0.0015);
            Assert.AreEqual(Math.Abs(Wel2 - sec.Wel2), 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wel1 - sec.Wel1), 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wpl2 - sec.Wpl2), 0, 0.0015);
            Assert.AreEqual(Math.Abs(Wpl1 - sec.Wpl1), 0, 0.0015);
        }

        [TestMethod]
        public void SectionRectangularTest03()
        {
            double h = 2.0;
            double b = 10.0;
            SectionRectangular sec = new SectionRectangular(h, b, 1.0);

            double A = h * b;
            double J2 = 1.0 / 12.0 * b * Math.Pow(h, 3.0);
            double J1 = 1.0 / 12.0 * h * Math.Pow(b, 3.0);
            double Jxx = 119.9584136;
            double Jyy = 53.37491974;
            double Jxy = 72.74379415;
            double Wel1 = 1.0 / 6.0 * b * Math.Pow(h, 2.0);
            double Wel2 = 1.0 / 6.0 * h * Math.Pow(b, 2.0);
            double Wpl1 = A / 2.0 * h / 2.0;
            double Wpl2 = A / 2.0 * b / 2.0;

            double precision = 0.0000001;

            Assert.AreEqual(0, A / sec.Area - 1.0, precision);
            Assert.AreEqual(0, J2 / sec.J22 - 1.0, precision);
            Assert.AreEqual(0, J1 / sec.J11 - 1.0, precision);
            Assert.AreEqual(0, Jxx / sec.Jxx - 1.0, precision);
            Assert.AreEqual(0, Jyy / sec.Jyy - 1.0, precision);
            Assert.AreEqual(0, Jxy / sec.Jxy - 1.0, precision);
            Assert.AreEqual(0, Wel2 / sec.Wel2 - 1.0, precision);
            Assert.AreEqual(0, Wel1 / sec.Wel1 - 1.0, precision);
            Assert.AreEqual(0, Wpl2 / sec.Wpl2 - 1.0, precision);
            Assert.AreEqual(0, Wpl1 / sec.Wpl1 - 1.0, precision);
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
            var sec = new SteelSection(new SectionRHS(h, b, tf, tf, tw, tw, string.Empty), SteelMaterialEN1993Data.S355);

            double A = 7200;
            double Jx = 39360000;
            double Jy = 9840000;
            double Welx = 393600;
            double Wely = 196800;
            double Wplx = 488000;
            double Wply = 244000;
            double Jt = 23328000;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(b - 2 * tw, ((SectionRHS)sec.SectionShape).BaseInternal);
            Assert.AreEqual(h - 2 * tf, ((SectionRHS)sec.SectionShape).Heightinternal);
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
            var sec = new SteelSection(new SectionRHS(h, b, tf, tf, tw, tw, string.Empty), SteelMaterialEN1993Data.S355);

            double A = 11600;
            double Jx = 243586666.67;
            double Jy = 81986666.67;
            double Wely = 819866.67;
            double Welx = 1217933.33;
            double Wplx = 1502000.000;
            double Wply = 922000.000;
            double Jt = 189338275.9;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(b - 2 * tw, ((SectionRHS)sec.SectionShape).BaseInternal);
            Assert.AreEqual(h - 2 * tf, ((SectionRHS)sec.SectionShape).Heightinternal);
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
            var sec = new SteelSection(new SectionRHS(h, b, tf, tf, tw, tw, string.Empty), SteelMaterialEN1993Data.S355);

            double A = 17100;
            double Jx = 232132500;
            double Jy = 232132500;
            double Wely = 1547550;
            double Welx = 1547550;
            double Wplx = 1829250;
            double Wply = 1829250;
            double Jt = 347236875;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(b - 2 * tw, ((SectionRHS)sec.SectionShape).BaseInternal);
            Assert.AreEqual(h - 2 * tf, ((SectionRHS)sec.SectionShape).Heightinternal);
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
            var sec = new SteelSection(new SectionRHS(h, b, tf, tf, tw, tw, string.Empty), SteelMaterialEN1993Data.S355);

            double A = 6250;
            double Jx = 18880208.33;
            double Jy = 13997395.83;

            Assert.AreEqual(A, sec.Area, 0.001);
            Assert.AreEqual(b - 2 * tw, ((SectionRHS)sec.SectionShape).BaseInternal);
            Assert.AreEqual(h - 2 * tf, ((SectionRHS)sec.SectionShape).Heightinternal);
            Assert.AreEqual(Math.Abs(Jy / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jx / sec.J11) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionRHS_Test5()
        {
            double h = 200;
            double b = 100;
            double tf_top = 20;
            double tf_bottom = 30;
            double tw_left = 10;
            double tw_right = 40;
            var sec = new SteelSection(new SectionRHS(h, b, tf_top, tf_bottom, tw_left, tw_right, string.Empty), SteelMaterialEN1993Data.S355);

            double A = 12500;
            double J11 = 52324457.10;
            double J22 = 12383876.23;
            double Wpl1 = 716605.5676;
            double Wpl2 = 335111.4542;

            double errorA = Error.CalcRelativeError(sec.Area, A);
            double errorJ11 = Error.CalcRelativeError(sec.J11, J11);
            double errorJ22 = Error.CalcRelativeError(sec.J22, J22);
            double errorWpl1 = Error.CalcRelativeError(sec.Wpl1, Wpl1);
            double errorWpl2 = Error.CalcRelativeError(sec.Wpl2, Wpl2);

            Assert.IsTrue(Math.Abs(errorA) < 1e-6);
            Assert.IsTrue(Math.Abs(errorJ22) < 1e-6);
            Assert.IsTrue(Math.Abs(errorJ11) < 1e-6);
            Assert.IsTrue(Math.Abs(errorWpl1) < 0.001);
            Assert.IsTrue(Math.Abs(errorWpl2) < 0.001);
        }

        [TestMethod]
        public void SectionRHS_Sigma1()
        {
            double h = 300;
            double b = 200;
            double t = 8;

            var sec = new SteelSection(new SectionRHS(h, b, t, t, t, t, string.Empty), SteelMaterialEN1993Data.S355);

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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty, radius), SteelMaterialEN1993Data.S355, Section.SectionTypes.Welded, Section.FormedTypes.HotFinished);

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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty, radius), SteelMaterialEN1993Data.S355, Section.SectionTypes.Welded, Section.FormedTypes.HotFinished);

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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty, radius), SteelMaterialEN1993Data.S355, Section.SectionTypes.Welded, Section.FormedTypes.HotFinished);

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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty, radius), SteelMaterialEN1993Data.S355, Section.SectionTypes.Welded, Section.FormedTypes.HotFinished);

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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty, radius), SteelMaterialEN1993Data.S355, Section.SectionTypes.Welded, Section.FormedTypes.HotFinished);

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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty), SteelMaterialEN1993Data.S355);

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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty), SteelMaterialEN1993Data.S355);

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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty, r), SteelMaterialEN1993Data.S355,
                Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty), SteelMaterialEN1993Data.S355);

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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty, radius), SteelMaterialEN1993Data.S355,
                Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty), SteelMaterialEN1993Data.S355);

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

            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty, r), SteelMaterialEN1993Data.S355,
                Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

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
            // IPE300
            double h = 300.0;
            double width = 150.0;
            double flangeThickness = 10.7;
            double webThickness = 7.1;
            double r = 15.0;

            var sec = new SteelSection(new SectionH(h, webThickness, width, flangeThickness, width, flangeThickness, string.Empty, r), SteelMaterialEN1993Data.S355,
                Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

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

        private static void SectionPropertiesIntegrals(SteelSection sec, double flexModule, Point2d centerID, out double JxxIntegral, out double JyyIntegral, out double JxyIntegral, out double Wel1Integral, out double Wpl1Integral)
        {
            double JxxFunction(double x, double y) => Math.Pow(y - centerID.Y, 2);
            double JyyFunction(double x, double y) => Math.Pow(x - centerID.X, 2);
            double JxyFunction(double x, double y) => (x - centerID.X) * (y - centerID.Y);
            double Wel1Function(double x, double y) => flexModule * Math.Pow(y - centerID.Y, 2);
            double Wpl1Function(double x, double y) => Math.Abs(y - centerID.Y);

            JxxIntegral = 0.0;
            JyyIntegral = 0.0;
            JxyIntegral = 0.0;
            Wel1Integral = 0.0;
            Wpl1Integral = 0.0;
            for (int i = 0; i < sec.SectionShape.ThinWalls.Length; i++)
            {
                var thinWall = sec.ThinWalls[i];
                var middleLine = thinWall.GetMiddleLine();

                Point3d[] middleLine3d = new Point3d[middleLine.Length];
                for (int j = 0; j < middleLine3d.Length; j++)
                    middleLine3d[j] = new Point3d(middleLine[j].X, middleLine[j].Y, 0.0);

                var thickness = thinWall.T;

                JxxIntegral += GaussIntegration.IntegrationLineLinearShapeFunction(JxxFunction, middleLine3d, LineGaussPoints.GaussPointNumber.Line20) * thickness;
                JyyIntegral += GaussIntegration.IntegrationLineLinearShapeFunction(JyyFunction, middleLine3d, LineGaussPoints.GaussPointNumber.Line20) * thickness;
                JxyIntegral += GaussIntegration.IntegrationLineLinearShapeFunction(JxyFunction, middleLine3d, LineGaussPoints.GaussPointNumber.Line20) * thickness;
                Wel1Integral += GaussIntegration.IntegrationLineLinearShapeFunction(Wel1Function, middleLine3d, LineGaussPoints.GaussPointNumber.Line20) * thickness;
                Wpl1Integral += GaussIntegration.IntegrationLineLinearShapeFunction(Wpl1Function, middleLine3d, LineGaussPoints.GaussPointNumber.Line20) * thickness;
            }
        }

        [TestMethod]
        [DataTestMethod]
        // Error on 2023-06-19 without fillet radius: direction 1 --> -0.000456188; direction 2 --> -0.00388512.
        // Error on 2023-06-19 with fillet radius: direction 1 --> -0.023036549; direction 2 --> -0.006735501.
        [DataRow("HEM1000", 302.0, 1008.0, 21.0, 40.0, 30.0,
            0.0005, 0.004, 0.025, 0.007)]

        // Error on 2023-06-19 without fillet radius: direction 1 --> -0.00205272; direction 2 --> -0.00179064.
        // Error on 2023-06-19 with fillet radius: direction 1 --> -0.023132658; direction 2 --> -0.003310338.
        [DataRow("HEM500", 306.0, 524.0, 21.0, 40.0, 27.0,
            0.0025, 0.002, 0.025, 0.004)]

        // Error on 2023-06-19 without fillet radius: direction 1 --> -0.005129053; direction 2 --> -0.001310936.
        // Error on 2023-06-19 with fillet radius: direction 1 --> -0.023132658; direction 2 --> -0.003310338.
        [DataRow("HEM200", 206.0, 220.0, 15.0, 25.0, 18.0,
            0.006, 0.002, 0.025, 0.004)]

        // Error on 2023-06-16 without fillet radius: direction 1 --> -0.012559242; direction 2 --> -0.002893329.
        // Error on 2023-06-16 with fillet radius: direction 1 --> -0.028942029; direction 2 --> -0.003.
        [DataRow("HEM100", 106.0, 120.0, 12.0, 20.0, 12.0,
            0.013, 0.003, 0.03, 0.007)]

        // Error on 2023-06-19 without fillet radius: direction 1 --> -0.000284737; direction 2 --> -0.002394348.
        // Error on 2023-06-19 with fillet radius: direction 1 --> -0.041645632; direction 2 --> -0.008250776.
        [DataRow("IPE600", 220.0, 600.0, 12.0, 19.0, 24.0,
            0.0003, 0.0025, 0.045, 0.009)]

        // Error on 2023-06-16 without fillet radius: direction 1 --> -0.000382873; direction 2 --> -0.001378616.
        // Error on 2023-06-16 with fillet radius: direction 1 --> -0.043; direction 2 --> -0.00683746.
        [DataRow("IPE300", 150.0, 300.0, 7.1, 10.7, 15.0,
            0.0004, 0.002, 0.05, 0.007)]

        // Error on 2023-06-19 without fillet radius: direction 1 --> -0.001039416; direction 2 --> -0.003209197.
        // Error on 2023-06-19 with fillet radius: direction 1 --> -0.047447987; direction 2 --> -0.012503161.
        [DataRow("IPE100", 55.0, 100.0, 4.1, 5.7, 7.0,
            0.0011, 0.0035, 0.05, 0.015)]

        // Error on 2023-06-19 without fillet radius: direction 1 --> -0.001387363; direction 2 --> -0.003758528.
        // Error on 2023-06-19 with fillet radius: direction 1 --> -0.032638598; direction 2 --> -0.008674766.
        [DataRow("IPE80", 46.0, 80.0, 3.8, 5.2, 5.0,
            0.0015, 0.004, 0.035, 0.009)]

        public void SectionHSymmetric_Test9(string _, double width, double height, double webThickness, double flangeThickness, double radius,
            double direction1ErrorLimit, double direction2ErrorLimit, double direction1ErrorLimitWithFillet, double direction2ErrorLimitWithFillet)
        {
            // Linear integration over thin walls.
            var secNoRadius = new SteelSection(new SectionH(height, webThickness, width, flangeThickness, width, flangeThickness,
                string.Empty, 0.0), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

            var sec = new SteelSection(new SectionH(height, webThickness, width, flangeThickness, width, flangeThickness,
                string.Empty, radius), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

            // Rigidity factor.
            double flexModule = 2.0 / height;

            var centerID = new Point2d(0.5 * width, 0.5 * height);

            SectionPropertiesIntegrals(secNoRadius, flexModule, centerID,
                out double JxxIntegral, out double JyyIntegral, out double JxyIntegral, out double Wel1Integral, out double Wpl1Integral);

            // Error without fillet radius (radius=0.0).
            Assert.AreEqual(secNoRadius.J11, JxxIntegral, secNoRadius.J11 * direction1ErrorLimit);
            Assert.AreEqual(secNoRadius.J22, JyyIntegral, secNoRadius.J22 * direction2ErrorLimit);
            Assert.AreEqual(secNoRadius.Jxy, JxyIntegral, 1);
            Assert.AreEqual(secNoRadius.Wel1, Wel1Integral, secNoRadius.Wel1 * direction1ErrorLimit);
            Assert.AreEqual(secNoRadius.Wpl1, Wpl1Integral, secNoRadius.Wpl1 * 0.001);

            // Error with fillet radius.
            Assert.AreEqual(sec.J11, JxxIntegral, sec.J11 * direction1ErrorLimitWithFillet);
            Assert.AreEqual(sec.J22, JyyIntegral, sec.J22 * direction2ErrorLimitWithFillet);
        }

        [TestMethod]
        public void SectionHSymmetric_OutPutPoints()
        {
            double h = 300.0;
            double width = 150.0;
            double flangeThickness = 20;
            double webThickness = 10;
            double r = 15.0;

            var sec = new SteelSection(new SectionH(h, webThickness, width, flangeThickness, width, flangeThickness,
                string.Empty, r), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

            var points = sec.SectionShape.GetSectionPoints();
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
            var sec = new SteelSection(new SectionH(h, tw, bt, tt, bb, tb, string.Empty), SteelMaterialEN1993Data.S355);

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
            SectionT sec = new SectionT(h, b, tw, tf, string.Empty);

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
            SectionT sec = new SectionT(h, b, tw, tf, string.Empty);

            //double JwLTBEAM = 39044 * 1e6; // --> WRONG
            double JwStraus = 1.32284 * 1e10;
            //double JwSAP = 13751083333;
            Assert.AreEqual(JwStraus / sec.Jw - 1.0, 0, 0.05);
        }

        [TestMethod]
        public void SectionT_Test3()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-1200, 0),
                new Point2d(1200, 0),
                new Point2d(1200, 550),
                new Point2d(325, 550),
                new Point2d(325, 3100),
                new Point2d(-325, 3100),
                new Point2d(-325, 550),
                new Point2d(-1200, 550),
            }));

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            Assert.IsTrue(Math.Abs(section.Centroid.X) < 1);
            Assert.IsTrue(Math.Abs(section.Centroid.Y - 1138) < 1);
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
            SteelSection sec = new SteelSection(new SectionC(h, tw, b, tf, b, tf, string.Empty), SteelMaterialEN1993Data.S355,
                Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

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
            SteelSection sec = new SteelSection(new SectionC(h, webThickness, width, flangeThickness, width, flangeThickness,
                string.Empty, radius1, radius2), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

            // Values without slope in the flange. p=0%.
            double A = 5962.40710526;
            double Jyy = 5539278.82938194;   // noi non consideriamo l'inclinazione
            double Jxx = 81997310.3;
            double Welx = 546649.0;
            double Wplx = 644561.0;
            double Jt = 374000.0;
            double Jw = 69100000000.0;

            Assert.AreEqual(Math.Abs(A / sec.Area - 1), 0, 0.02);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22 - 1), 0, 0.04);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11 - 1), 0, 0.02);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.04);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.02);
            Assert.AreEqual(Math.Abs(Welx / sec.Wel1) - 1, 0, 0.02);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.02);
            Assert.AreEqual(Jt / sec.Jt - 1.0, 0, 0.057);
            Assert.AreEqual(Jw / sec.Jw - 1, 0, 0.14);
        }

        [TestMethod]
        public void SectionC_Test3()
        {
            // UPN 300 ArcelorMittal
            double h = 300.0;
            double width = 100.0;
            double flangeThickness = 16.0;
            double webThickness = 10.0;
            //double radius1 = 16.0;
            //double radius2 = 8.0;
            SteelSection sec = new SteelSection(new SectionC(h, webThickness, width, flangeThickness, width, flangeThickness,
                string.Empty, 0.0, 0.0), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

            double A = 5880;
            double Jyy = 5642469.38775510;
            double Jxx = 80633760.0;
            double Welx = 537558.4;
            double Wplx = 633960.0;

            Assert.AreEqual(Math.Abs(A / sec.Area - 1), 0, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.J22 - 1), 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.J11 - 1), 0, 0.001);
            Assert.AreEqual(Math.Abs(Jyy / sec.Jyy) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Jxx / sec.Jxx) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Welx / sec.Wel1) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(Wplx / sec.Wpl1) - 1, 0, 0.001);
        }

        [TestMethod]
        public void SectionC_Test4()
        {
            // Section, without symmetries.
            double h = 200.0;
            double widthBottom = 100.0;
            double widthTop = 50.0;
            double flangeBottomThickness = 30.0;
            double flangeTopThickness = 20.0;
            double webThickness = 10.0;

            SteelSection sec = new SteelSection(new SectionC(h, webThickness, widthTop, flangeTopThickness, widthBottom, flangeBottomThickness,
                string.Empty, 0.0, 0.0), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

            double A = 5500;
            double J22 = 3715635.423;
            double J11 = 29485122.15;
            double Wpl1 = 364013.9331;
            double Wpl2 = 122941.4776;

            double errorA = Error.CalcRelativeError(sec.Area, A);
            double errorJ22 = Error.CalcRelativeError(sec.J22, J22);
            double errorJ11 = Error.CalcRelativeError(sec.J11, J11);
            double errorWpl1 = Error.CalcRelativeError(sec.Wpl1, Wpl1);
            double errorWpl2 = Error.CalcRelativeError(sec.Wpl2, Wpl2);

            Assert.IsTrue(Math.Abs(errorA) < 1e-6);
            Assert.IsTrue(Math.Abs(errorJ22) < 1e-6);
            Assert.IsTrue(Math.Abs(errorJ11) < 1e-6);
            Assert.IsTrue(Math.Abs(errorWpl1) < 0.001);
            Assert.IsTrue(Math.Abs(errorWpl2) < 0.002);
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
            var sec = new SteelSection(new SectionL(b, tb, h, tw, string.Empty), SteelMaterialEN1993Data.S355);

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
            var sec = new SteelSection(new SectionL(b, tb, h, tw, string.Empty), SteelMaterialEN1993Data.S355);

            double Wel2 = 1.0 / 6.0 * b * tb * tb;
            double Wel1 = 1.0 / 6.0 * tb * b * b;

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
            var sec = new SteelSection(new SectionL(b, tb, h, tw, string.Empty), SteelMaterialEN1993Data.S355);

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
            var sec = new SteelSection(new SectionL(b, tb, h, tw, string.Empty), SteelMaterialEN1993Data.S355);

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
            Assert.IsTrue(Math.Abs(sec.AngleX1 - angle) < 0.001);
        }

        [TestMethod]
        public void SectionL_Test5()
        {
            double h = 200;
            double tw = 10;
            double b = 200;
            double tb = 20;
            var sec = new SteelSection(new SectionL(b, tb, h, tw, string.Empty), SteelMaterialEN1993Data.S355);

            double A = 5800;
            double jyy = 24551782;
            double jxx = 17407126;
            double j11 = 33301743.312114567;
            double j22 = 8657164.733862447;
            double angle = 0.9324634048;

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
            var sec = new SteelSection(new SectionL(b, tb, h, tw, string.Empty), SteelMaterialEN1993Data.S355);

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
            var sec = new SteelSection(new SectionL(b, tb, h, tw, string.Empty), SteelMaterialEN1993Data.S355);

            double A = 38400;
            double J2 = 375037668.5153;
            double J1 = 1477129772.799;
            double teta = Math.PI / 4.0;

            Assert.AreEqual(A, sec.Area);
            Assert.AreEqual(Math.Abs(J2 / sec.J22) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(J1 / sec.J11) - 1, 0, 0.001);
            Assert.AreEqual(Math.Abs(teta / sec.AngleX1) - 1, 0, 0.001);
            Assert.IsTrue(Math.Abs(sec.AngleX1 - teta) < 0.001);
        }

        [TestMethod]
        [DataTestMethod]
        // Error on 2023-06-19 without fillet radius: -0.021216407.
        [DataRow("L30x6", 30.0, 6.0, 30.0, 6.0, 5.0, 0.025)]

        // Error on 2023-06-19 without fillet radius: -0.012878111.
        [DataRow("L100x16", 100.0, 16.0, 100.0, 16.0, 12.0, 0.015)]

        public void SectionL_Test8(string description, double lHor, double tHor, double lVert, double tVert, double radius, double errorLimit)
        {
            // Linear integration over thin walls.
            var secNoRadius = new SteelSection(new SectionL(lHor, tHor, lVert, tVert,
                string.Empty, 0.0), SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);

            var centerID = secNoRadius.Centroid;

            // Rigidity factor.
            double flexModule = 1.0 / (lVert - centerID.Y);

            SectionPropertiesIntegrals(secNoRadius, flexModule, centerID,
                out double JxxIntegral, out double JyyIntegral, out double JxyIntegral, out double Wel1Integral, out double _);

            // Error without fillet radius (radius=0.0).
            Assert.AreEqual(secNoRadius.Jxx, JxxIntegral, secNoRadius.Jxx * errorLimit);
            Assert.AreEqual(secNoRadius.Jyy, JyyIntegral, secNoRadius.Jyy * errorLimit);
            Assert.AreEqual(secNoRadius.Jxy, JxyIntegral, 1);
            Assert.AreEqual(secNoRadius.WelXMax, Wel1Integral, secNoRadius.WelXMax * errorLimit);
            //Assert.AreEqual(secNoRadius.Wpl1, Wpl1Integral, secNoRadius.Wpl1 * 0.001);
        }

        [TestMethod]
        public void SectionL_Test9()
        {
            double h = 200;
            double tw = 10;
            double b = 100;
            double tb = 30;
            var sec = new SteelSection(new SectionL(b, tb, h, tw, string.Empty), SteelMaterialEN1993Data.S355);

            double A = 4700;
            double J2 = 2786170.686;
            double J1 = 17095566.90;
            double teta = 0.3755790289;
            double Wpl1 = 238630.2014;
            double Wpl2 = 97207.88541;

            double errorA = Error.CalcRelativeError(sec.Area, A);
            double errorJ22 = Error.CalcRelativeError(sec.J22, J2);
            double errorJ11 = Error.CalcRelativeError(sec.J11, J1);
            double errorTeta = Error.CalcRelativeError(sec.AngleX1, teta);
            double errorWpl1 = Error.CalcRelativeError(sec.Wpl1, Wpl1);
            double errorWpl2 = Error.CalcRelativeError(sec.Wpl2, Wpl2);

            Assert.IsTrue(Math.Abs(errorA) < 1e-6);
            Assert.IsTrue(Math.Abs(errorJ22) < 1e-6);
            Assert.IsTrue(Math.Abs(errorJ11) < 1e-6);
            Assert.IsTrue(Math.Abs(errorTeta) < 1e-6);
            Assert.IsTrue(Math.Abs(errorWpl1) < 0.001);
            Assert.IsTrue(Math.Abs(errorWpl2) < 0.006);
        }

        #endregion

        #region Section Rectangular

        [TestMethod]
        public void SectionRectangularTest1()
        {
            double h = 500;
            double b = 300;
            var section = new ReinforcedConcreteSection(new SectionRectangular(h, b, "Section"), ConcreteMaterialEN1992Data.C40_50);

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
            var section = new ReinforcedConcreteSection(new SectionRectangular(h, b, "Section"), ConcreteMaterialEN1992Data.C40_50);

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
            var section = new ReinforcedConcreteSection(new SectionCircular(d, "Section"), ConcreteMaterialEN1992Data.C40_50);

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
            var section = new ReinforcedConcreteSection(new SectionCircular(d, "Section"), ConcreteMaterialEN1992Data.C40_50);

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
        public void SectionGenericTest2()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(0, 0),
                new Point2d(400, 0),
                new Point2d(400, 400),
                new Point2d(0, 380),
            }));

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            double x = 201.7;
            double y = 195.0;
            double angleDeg = -66.89;

            Assert.IsTrue(Math.Abs(section.Centroid.X - x) < 1);
            Assert.IsTrue(Math.Abs(section.Centroid.Y - y) < 1);
            Assert.IsTrue(Math.Abs(section.AngleX1 - angleDeg.ToRadians()) < 0.1);
        }

        [TestMethod]
        public void SectionGenericTest3()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(0, 0),
                new Point2d(400, 0),
                new Point2d(400, 400),
                new Point2d(0, 100),
            }));

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            double x = 240;
            double y = 140;
            double angleDeg = -54.41;

            Assert.IsTrue(Math.Abs(section.Centroid.X - x) < 1);
            Assert.IsTrue(Math.Abs(section.Centroid.Y - y) < 1);
            Assert.IsTrue(Math.Abs(section.AngleX1 - angleDeg.ToRadians()) < 0.1);
        }

        [TestMethod]
        public void SectionGenericTest4()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(0, 0),
                new Point2d(1000, 0),
                new Point2d(1000, 400),
                new Point2d(200, 200),
                new Point2d(0, 400),
                new Point2d(-50, 50),
            }));

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            double x = 515.6;
            double y = 155.4;
            double angleDeg = -86.93;

            Assert.IsTrue(Math.Abs(section.Centroid.X - x) < 1);
            Assert.IsTrue(Math.Abs(section.Centroid.Y - y) < 1);
            Assert.IsTrue(Math.Abs(section.AngleX1 - angleDeg.ToRadians()) < 0.1);
        }

        [TestMethod]
        public void SectionGenericTest5()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(0, 0),
                new Point2d(1000, 0),
                new Point2d(1000, 400),
                new Point2d(200, 200),
            }));

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            double x = 605.1;
            double y = 148.7;
            double angleDeg = -81.02;

            Assert.IsTrue(Math.Abs(section.Centroid.X - x) < 1);
            Assert.IsTrue(Math.Abs(section.Centroid.Y - y) < 1);
            Assert.IsTrue(Math.Abs(section.AngleX1 - angleDeg.ToRadians()) < 0.1);
        }

        [TestMethod]
        public void SectionGenericTest6()
        {
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(0, 0),
                new Point2d(1000, 0),
                new Point2d(1000, 400),
                new Point2d(200, 200),
            }));

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            double x = 605.1;
            double y = 148.7;
            double angleDeg = -81.02;

            Assert.IsTrue(Math.Abs(section.Centroid.X - x) < 1);
            Assert.IsTrue(Math.Abs(section.Centroid.Y - y) < 1);
            Assert.IsTrue(Math.Abs(section.AngleX1 - angleDeg.ToRadians()) < 0.1);
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
            Shape2d shape = GetRectangularShape2d(width, heigth);
            RebarSectionCircular rebar = new RebarSectionCircular(rebarDiameter, SteelMaterialEN1992Data.B450C);

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] {
                new ReinforcedConcreteRebar(rebar, new Point2d(50, 50)),
                new ReinforcedConcreteRebar(rebar, new Point2d(250, 50)),
                new ReinforcedConcreteRebar(rebar, new Point2d(250, 450)),
                new ReinforcedConcreteRebar(rebar, new Point2d(50, 450))};
            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);
            section.AddRebars(rebars);

            double phi = n * section.ConcreteMaterial.ElasticModulusCompression / section.GetRebars().FirstOrDefault().RebarMaterial.ElasticModulusCompression - 1;
            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(phi);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(section.Area - 150000) / section.Area * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jxx - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J11 - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J22 - 1125000000) / section.J22 * 100 < 1);
            Assert.AreEqual(0, section.AngleX1, 0.001);

            Assert.AreEqual(165270 - rebars.Select(i => i.Area).Sum(), mechanicalProperties.areaH, 2);

            var expectedJ11H = 3735800000 - section.GetRebars().Select(i => (i.RebarSection.Jxx + i.Area * Math.Pow(i.Position.Y - section.Centroid.Y, 2))).Sum();
            var expectedJ22H = 1277700000 - section.GetRebars().Select(i => (i.RebarSection.Jyy + i.Area * Math.Pow(i.Position.X - section.Centroid.X, 2))).Sum();

            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ11H) / section.GetHomogeneizedJ11(phi)) < 0.005);
            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ22H) / section.GetHomogeneizedJ22(phi)) < 0.005);
            Assert.AreEqual(0, mechanicalProperties.angleX, 0.001);

            var sectionRectangular = new ReinforcedConcreteSection(new SectionRectangular(heigth, width), ConcreteMaterialEN1992Data.C25_30);
            sectionRectangular.AddRebars(rebars);

            mechanicalProperties = section.GetHomogeneizedMechanicalProperties(phi);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(sectionRectangular.Area - 150000) / section.Area * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jxx - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(sectionRectangular.J11 - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(sectionRectangular.J22 - 1125000000) / section.J22 * 100 < 1);
            Assert.AreEqual(0, sectionRectangular.AngleX1, 0.001);
            Assert.AreEqual(165270 - sectionRectangular.GetRebars().Select(i => i.Area).Sum(), mechanicalProperties.areaH, 2);

            expectedJ11H = 3735800000 - sectionRectangular.GetRebars().Select(i => (i.RebarSection.Jxx + i.Area * Math.Pow(i.Position.Y - section.Centroid.Y, 2))).Sum();
            expectedJ22H = 1277700000 - sectionRectangular.GetRebars().Select(i => (i.RebarSection.Jyy + i.Area * Math.Pow(i.Position.X - section.Centroid.X, 2))).Sum();

            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ11H) / section.GetHomogeneizedJ11(phi)) < 0.005);
            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ22H) / section.GetHomogeneizedJ22(phi)) < 0.005);
            Assert.AreEqual(0, mechanicalProperties.angleX, 0.001);
        }

        [TestMethod]
        public void RCRectangularSection2()
        {
            double heigth = 500;
            double width = 300;
            double rebarDiameter = 18;
            double n = 15;

            // sezione rettangolare 300x500
            Shape2d shape = GetRectangularShape2d(width, heigth);

            RebarSectionCircular rebar = new RebarSectionCircular(rebarDiameter, SteelMaterialEN1992Data.B450C);

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] {
                new ReinforcedConcreteRebar(rebar, new Point3d(50,50,0)),
                new ReinforcedConcreteRebar(rebar, new Point3d(100, 50, 0)),
                new ReinforcedConcreteRebar(rebar, new Point3d(200, 50,0)),
                new ReinforcedConcreteRebar(rebar, new Point3d(250, 50,0))};

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);
            section.AddRebars(rebars);

            double phi = n * section.ConcreteMaterial.ElasticModulusCompression / section.GetRebars().FirstOrDefault().RebarMaterial.ElasticModulusCompression - 1;
            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(phi);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(section.J11 - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J22 - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jxx - 3125000000) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 1125000000) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J11H - 3644680436) / mechanicalProperties.J11H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.J22H - 1213900000) / mechanicalProperties.J22H * 100 < 1);
            Assert.IsTrue(Math.Abs(mechanicalProperties.angleX) < 0.001);

            var sectionRectangular = new ReinforcedConcreteSection(new SectionRectangular(heigth, width), ConcreteMaterialEN1992Data.C25_30);
            sectionRectangular.AddRebars(rebars);

            mechanicalProperties = sectionRectangular.GetHomogeneizedMechanicalProperties(phi);

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

            Shape2d shape = GetRectangularShape2d(width, heigth);

            RebarSectionCircular rebar = new RebarSectionCircular(rebarDiameter, SteelMaterialEN1992Data.B450C);

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] {
                new ReinforcedConcreteRebar(rebar, new Point3d(50,50,0)),
                new ReinforcedConcreteRebar(rebar, new Point3d(100, 50, 0)),
                new ReinforcedConcreteRebar(rebar, new Point3d(200, 50,0)),
                new ReinforcedConcreteRebar(rebar, new Point3d(250, 50,0)),
                new ReinforcedConcreteRebar(rebar, new Point3d(50,450,0)),
                new ReinforcedConcreteRebar(rebar, new Point3d(100, 450, 0)),
                new ReinforcedConcreteRebar(rebar, new Point3d(200, 450,0)),
                new ReinforcedConcreteRebar(rebar, new Point3d(250, 450,0))};

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);
            section.AddRebars(rebars);

            double phi = n * section.ConcreteMaterial.ElasticModulusCompression / section.GetRebars().FirstOrDefault().RebarMaterial.ElasticModulusCompression - 1;
            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(phi);

            //valori calcolati con VCASLU
            Assert.AreEqual(174132 - section.GetRebars().Select(i => i.Area).Sum(), mechanicalProperties.areaH, 5);

            var expectedJ11H = 4090280029 - section.GetRebars().Select(i => (i.RebarSection.Jxx + i.Area * Math.Pow(i.Position.Y - section.Centroid.Y, 2))).Sum();
            var expectedJ22H = 1259063991 - section.GetRebars().Select(i => (i.RebarSection.Jyy + i.Area * Math.Pow(i.Position.X - section.Centroid.X, 2))).Sum();

            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ11H) / mechanicalProperties.J11H) < 0.005);
            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ22H) / mechanicalProperties.J22H) < 0.015);
            Assert.AreEqual(0, mechanicalProperties.angleX, 0.001);

            var sectionRectangular = new ReinforcedConcreteSection(new SectionRectangular(heigth, width), ConcreteMaterialEN1992Data.C25_30);
            sectionRectangular.AddRebars(rebars);

            mechanicalProperties = sectionRectangular.GetHomogeneizedMechanicalProperties(phi);

            //valori calcolati con VCASLU
            Assert.AreEqual(174132 - sectionRectangular.GetRebars().Select(i => i.Area).Sum(), mechanicalProperties.areaH, 5);

            expectedJ11H = 4090280029 - sectionRectangular.GetRebars().Select(i => (i.RebarSection.Jxx + i.Area * Math.Pow(i.Position.Y - section.Centroid.Y, 2))).Sum();
            expectedJ22H = 1259063991 - sectionRectangular.GetRebars().Select(i => (i.RebarSection.Jyy + i.Area * Math.Pow(i.Position.X - section.Centroid.X, 2))).Sum();

            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ11H) / mechanicalProperties.J11H) < 0.005);
            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ22H) / mechanicalProperties.J22H) < 0.015);
            Assert.AreEqual(0, mechanicalProperties.angleX, 0.001);
        }

        [TestMethod]
        public void RCRectangularSection4()
        {
            double heigth = 400;

            Shape2d shape = GetRectangularShape2d(heigth, heigth);
            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            Assert.IsTrue(Math.Abs(section.AngleX1) < 0.01);
        }

        [TestMethod]
        public void RCRectangularSection5()
        {
            double heigth = 600;

            Shape2d shape = GetRectangularShape2d(heigth, heigth);
            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            Assert.IsTrue(Math.Abs(section.AngleX1) < 0.01);
        }

        [TestMethod]
        public void RCGenericSection1()
        {
            // sezion generica a 4 punti
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[] {
                new Point2d(0, 0),
                new Point2d(500, 100),
                new Point2d(400, 300),
                new Point2d(100, 200) }));

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] { };

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);
            section.AddRebars(rebars);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(section.Jxx - 330208333) / section.Jxx * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 1127777778) / section.Jyy * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J11 - 1211051091) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J22 - 246935021) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.AngleX1 - (-72.91.ToRadians())) < 0.001);
        }

        [TestMethod]
        public void RCGenericSection2()
        {
            // sezion generica a 4 punti
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[] {
                new Point2d(0, 0),
                new Point2d(200, 0),
                new Point2d(200, 40),
                new Point2d(60, 40),
                new Point2d(60, 150),
                new Point2d(0, 150),
            }));

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(section.Jxx - 28064132) / section.Jxx * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 46367215) / section.Jyy * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J11 - 58292446) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J22 - 16138901) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.AngleX1 - (-122.1.ToRadians() + Math.PI)) < 0.001);
        }

        [TestMethod]
        public void RCTSection1()
        {
            // sezion a T tovescia 
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[] {
                new Point2d(0, 0),
                new Point2d(500, 0),
                new Point2d(500, 500),
                new Point2d(400, 500),
                new Point2d(400, 1000),
                new Point2d(100, 1000),
                new Point2d(100, 500),
                new Point2d(0, 500) }));

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] { };

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);
            section.AddRebars(rebars);

            //valori calcolati con VCASLU
            Assert.IsTrue(Math.Abs(section.Jxx - 31770833333) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.Jyy - 6333333333) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J11 - 31770833333) / section.J11 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.J22 - 6333333333) / section.J22 * 100 < 1);
            Assert.IsTrue(Math.Abs(section.AngleX1) < 0.001);
        }

        [TestMethod]
        public void RCTSection2()
        {
            // sezion a T tovescia 
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[] {
                new Point2d(0, 0),
                new Point2d(500, 0),
                new Point2d(500, 400),
                new Point2d(350, 400),
                new Point2d(350, 800),
                new Point2d(150, 800),
                new Point2d(150, 400),
                new Point2d(0, 400) }));

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            Assert.IsTrue(Math.Abs(section.AngleX1) < 0.001, section.AngleX1.ToString());
        }

        [TestMethod]
        public void RCTSection3()
        {
            // sezion a T tovescia 
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-800, 0),
                new Point2d(800, 0),
                new Point2d(800, 550),
                new Point2d(200, 550),
                new Point2d(200, 4500),
                new Point2d(-200, 4500),
                new Point2d(-200, 550),
                new Point2d(-800, 550),
            }));

            var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);
            Assert.IsTrue(Math.Abs(section.AngleX1 - 0) < 0.001, section.AngleX1.ToString());
        }

        [TestMethod]
        public void RCCircularSection1()
        {
            double rebarDiameter = 16;
            double diameter = 500;
            double n = 16;

            RebarSectionCircular rebar = new RebarSectionCircular(rebarDiameter, SteelMaterialEN1992Data.B450C);

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[] {
                new ReinforcedConcreteRebar(rebar, new Point2d(450, 250), 0),
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

            var section = new ReinforcedConcreteSection(new SectionCircular(diameter), ConcreteMaterialEN1992Data.C25_30);
            section.AddRebars(rebars);

            double phi = n * section.ConcreteMaterial.ElasticModulusCompression / section.GetRebars().FirstOrDefault().RebarMaterial.ElasticModulusCompression - 1;
            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(phi);

            //valori calcolati con VCASLU
            Assert.AreEqual(4082500718, mechanicalProperties.J11H, 1e8);
            Assert.AreEqual(4082500718, mechanicalProperties.J22H, 1e8);
            Assert.AreEqual(0, mechanicalProperties.angleX, 1e8);

            section = new ReinforcedConcreteSection(new SectionCircular(diameter), ConcreteMaterialEN1992Data.C25_30);

            section.AddRadialRebars(500.0, 50.0, rebars.Length, rebar);


            mechanicalProperties = section.GetHomogeneizedMechanicalProperties(phi);

            //valori calcolati con VCASLU
            Assert.AreEqual(4082500718, mechanicalProperties.J11H, 1e8);
            Assert.AreEqual(4082500718, mechanicalProperties.J22H, 1e8);
            Assert.AreEqual(0, mechanicalProperties.angleX, 1e8);
        }

        [TestMethod]
        public void RCCircularSection2()
        {
            double diameter = 500;

            var section = new ReinforcedConcreteSection(new SectionCircular(diameter), ConcreteMaterialEN1992Data.C25_30);

            Assert.IsTrue(Math.Abs(section.AngleX1) < 0.01);
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

            RebarSectionCircular rebar = new RebarSectionCircular(rebarDiameter, SteelMaterialEN1992Data.B450C);

            var section = new ReinforcedConcreteSection(new SectionCHS(diameterExternal, thickness), ConcreteMaterialEN1992Data.C25_30);
            section.AddRadialRebars(diameterExternal, concreteCover, numberOfRebars, rebar);

            double phi = n * section.ConcreteMaterial.ElasticModulusCompression / section.GetRebars().FirstOrDefault().RebarMaterial.ElasticModulusCompression - 1;
            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(phi);

            //valori calcolati con VCASLU a cui viene tolta la parte di cls sostituita dalla barra

            Assert.AreEqual(176830 - section.GetRebars().Select(i => i.Area).Sum(), mechanicalProperties.areaH, 350);

            var expectedJ11H = 3686823706 - section.GetRebars().Select(i => (i.RebarSection.Jxx + i.Area * Math.Pow(i.Position.Y - section.Centroid.Y, 2))).Sum();
            var expectedJ22H = 3686823706 - section.GetRebars().Select(i => (i.RebarSection.Jyy + i.Area * Math.Pow(i.Position.X - section.Centroid.X, 2))).Sum();

            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ11H) / mechanicalProperties.J11H) < 0.005);
            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ22H) / mechanicalProperties.J22H) < 0.005);
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

            RebarSectionCircular rebar = new RebarSectionCircular(rebarDiameter, SteelMaterialEN1992Data.B450C);

            var section = new ReinforcedConcreteSection(new SectionCHS(diameterExternal, thickness), ConcreteMaterialEN1992Data.C25_30);
            section.AddRadialRebars(diameterExternal, concreteCover, numberOfRebars, rebar);

            double phi = n * section.ConcreteMaterial.ElasticModulusCompression / section.GetRebars().FirstOrDefault().RebarMaterial.ElasticModulusCompression - 1;

            var mechanicalProperties = section.GetHomogeneizedMechanicalProperties(phi);

            var expectedJ11H = 56363826745 - section.GetRebars().Select(i => (i.RebarSection.Jxx + i.Area * Math.Pow(i.Position.Y - section.Centroid.Y, 2))).Sum();
            var expectedJ22H = 56363826745 - section.GetRebars().Select(i => (i.RebarSection.Jyy + i.Area * Math.Pow(i.Position.X - section.Centroid.X, 2))).Sum();

            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ11H) / mechanicalProperties.J11H) < 0.005);
            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ22H) / mechanicalProperties.J22H) < 0.005);
        }

        [TestMethod]
        public void RCCHSSection3()
        {
            double rebarDiameter = 8;
            double externalDiameter = 500;
            double thickness = 100;
            double concreteCover = 50;
            int numberOfRebars = 12;
            int discretization = 128;

            ConcreteMaterialModelCode2010 material = ConcreteMaterialModelCode2010Data.C28_35;
            SteelMaterial steelMaterial = SteelMaterialEN1992Data.B450C;

            Polygon2d fill = new Polygon2d(externalDiameter, discretization);
            Polygon2d hole = new Polygon2d(externalDiameter - 2 * thickness, discretization);

            fill.Move(250, 250, 0);
            hole.Move(250, 250, 0);

            Shape2d shape2D = new Shape2d(fill, new Polygon2d[] { hole });
            var section = new ReinforcedConcreteSection(shape2D, material);

            Polygon2d rebarPolygon = new Polygon2d(externalDiameter - concreteCover * 2.0, numberOfRebars, section.Centroid);
            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[rebarPolygon.Count];

            RebarSectionCircular rebarSection = new RebarSectionCircular(rebarDiameter, steelMaterial);
            for (int i = 0; i < rebarPolygon.Count; i++)
                rebars[i] = new ReinforcedConcreteRebar(rebarSection, rebarPolygon[i]);
            section.AddRebars(rebars);

            var mechanicalPropertiesH = section.GetHomogeneizedMechanicalProperties(1);

            double n = ConcreteSectionHelper.CalculateN(rebars.First(), material);
            double phi = n * section.ConcreteMaterial.ElasticModulusCompression / section.GetRebars().FirstOrDefault().RebarMaterial.ElasticModulusCompression - 1;

            Console.WriteLine($"n: {n}");
            Console.WriteLine($"phi: {phi}");
            Console.WriteLine(section.Jxy);
            Console.WriteLine(section.Rxy);

            double expectedArea = Math.PI * externalDiameter * externalDiameter / 4.0 - Math.PI * (externalDiameter - 2 * thickness) * (externalDiameter - 2 * thickness) / 4.0;
            double expectedI = Math.PI * Math.Pow(externalDiameter, 4) / 64.0 - Math.PI * Math.Pow(externalDiameter - 2 * thickness, 4) / 64.0;
            double expectedAreah = expectedArea + rebars.Select(i => i.Area * (n - 1)).Sum();

            Assert.AreEqual(expectedArea, section.Area, 60);
            Assert.AreEqual(expectedI, section.J11, 1e7);
            Assert.AreEqual(expectedI, section.J22, 1e7);
            Assert.AreEqual(250, section.Centroid.X, 0.001);
            Assert.AreEqual(250, section.Centroid.Y, 0.001);
            Assert.AreEqual(0, section.AngleX1, 0.001);
            Assert.AreEqual(expectedAreah, section.GetHomogenizedArea(phi), 60);

            var expectedJ11H = 2732065437 - section.GetRebars().Select(i => (i.RebarSection.Jxx + i.Area * Math.Pow(i.Position.Y - section.Centroid.Y, 2))).Sum();
            var expectedJ22H = 2732065437 - section.GetRebars().Select(i => (i.RebarSection.Jyy + i.Area * Math.Pow(i.Position.X - section.Centroid.X, 2))).Sum();

            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ11H) / section.GetHomogeneizedJ11(phi)) < 0.005);
            Assert.IsTrue(Math.Abs(1 - Math.Abs(expectedJ22H) / section.GetHomogeneizedJ11(phi)) < 0.005);
            Assert.AreEqual(0, mechanicalPropertiesH.angleX, 0.1);
            Assert.AreEqual(250, mechanicalPropertiesH.centroidH.X, 0.1);
            Assert.AreEqual(250, mechanicalPropertiesH.centroidH.Y, 0.1);
        }


        [TestMethod]
        public void HomogenizedProperties()
        {
            var material = ConcreteMaterialModelCode2010Data.C28_35;
            var steelMaterial = SteelMaterialEN1992Data.B450C;

            double width = 200;
            double height = 500;
            double cover = 50;

            double rebarDiameter = 12;
            double phi = 1;

            var section = new ReinforcedConcreteSection(new SectionRectangular(height, width), material);
            RebarSectionCircular rebarSection = new RebarSectionCircular(rebarDiameter, steelMaterial);

            section.AddRebar(new ReinforcedConcreteRebar(rebarSection, new Point2d(cover, cover)));
            section.AddRebar(new ReinforcedConcreteRebar(rebarSection, new Point2d(width - cover, cover)));
            section.AddRebar(new ReinforcedConcreteRebar(rebarSection, new Point2d(cover, height - cover)));
            section.AddRebar(new ReinforcedConcreteRebar(rebarSection, new Point2d(width - cover, height - cover)));

            var rebars = section.Rebars.ToArray();
            var mesh = section.Mesh;

            double expectedN = rebarSection.RebarMaterial.ElasticModulusCompression / section.ConcreteMaterial.ElasticModulusCompression;
            double expectedNMod = rebarSection.RebarMaterial.ElasticModulusCompression / (section.ConcreteMaterial.ElasticModulusCompression / (1.0 + phi));

            double expectedArea = width * height;
            double expectedAreaH = expectedArea + rebars.Select(i => i.Area * (expectedN - 1)).Sum();
            double expectedAreaH1 = expectedArea + rebars.Select(i => i.Area * (expectedNMod - 1)).Sum();

            double expectedSx = expectedArea * height / 2.0;
            double expectedSy = expectedArea * width / 2.0;

            double expectedSxH = expectedSx + rebars.Select(i => i.Area * i.Position.Y * (expectedN - 1)).Sum();
            double expectedSyH = expectedSy + rebars.Select(i => i.Area * i.Position.X * (expectedN - 1)).Sum();

            double expectedJxx = 1 / 12.0 * width * Math.Pow(height, 3);
            double expectedJyy = 1 / 12.0 * height * Math.Pow(width, 3);
            double expectedJxy = 0;
            double expectedJp = 1 / 12.0 * width * height * (Math.Pow(height, 2) + Math.Pow(width, 2));

            double expectedJxxH = expectedJxx + rebars.Select(i => (i.RebarSection.Jxx + i.Area * Math.Pow(i.Position.Y - section.Centroid.Y, 2)) * (expectedN - 1)).Sum();
            double expectedJyyH = expectedJyy + rebars.Select(i => (i.RebarSection.Jyy + i.Area * Math.Pow(i.Position.X - section.Centroid.X, 2)) * (expectedN - 1)).Sum();
            double expectedJxyH = expectedJxy + rebars.Select(i => (i.RebarSection.Jxy + i.Area * (i.Position.X - section.Centroid.X) * (i.Position.Y - section.Centroid.Y) * (expectedN - 1))).Sum();
            double expectedJpH = expectedJxxH + expectedJyyH;

            double expectedJxxH1 = expectedJxx + rebars.Select(i => (i.RebarSection.Jxx + i.Area * Math.Pow(i.Position.Y - section.Centroid.Y, 2)) * (expectedNMod - 1)).Sum();
            double expectedJyyH1 = expectedJyy + rebars.Select(i => (i.RebarSection.Jyy + i.Area * Math.Pow(i.Position.X - section.Centroid.X, 2)) * (expectedNMod - 1)).Sum();
            double expectedJxyH1 = expectedJxy + rebars.Select(i => (i.RebarSection.Jxy + i.Area * (i.Position.X - section.Centroid.X) * (i.Position.Y - section.Centroid.Y) * (expectedNMod - 1))).Sum();
            double expectedJpH1 = expectedJxxH1 + expectedJyyH1;

            Assert.AreEqual(expectedN, ConcreteSectionHelper.CalculateN(rebars.First(), section.ConcreteMaterial), 0.0001);
            Assert.AreEqual(expectedNMod, ConcreteSectionHelper.CalculateHomogenizedFactorN(phi, rebars.First(), section.ConcreteMaterial), 0.0001);
            Assert.AreEqual(expectedNMod, ConcreteSectionHelper.CalculateHomogenizedFactorN(phi, rebars, section.ConcreteMaterial), 0.0001);

            Assert.AreEqual(expectedAreaH, ConcreteSectionHelper.GetHomogenizedArea(rebars, section.ConcreteMaterial, expectedArea), 0.0001);
            Assert.AreEqual(expectedAreaH1, ConcreteSectionHelper.GetHomogenizedArea(phi, rebars, section.ConcreteMaterial, expectedArea), 0.0001);

            SectionHelper.CalculateStaticMoments(mesh, out double Sx, out double Sy);
            Assert.AreEqual(expectedSx, Sx, 0.0001);
            Assert.AreEqual(expectedSy, Sy, 0.0001);

            ConcreteSectionHelper.CalculateHomogeneizedStaticMoments(mesh, rebars, material, out double SxH, out double SyH);
            Assert.AreEqual(expectedSxH, SxH, 0.0001);
            Assert.AreEqual(expectedSyH, SyH, 0.0001);

            SectionHelper.CalculateInertiaMoments(mesh, section.Centroid, out double jxx, out double jyy, out double jxy, out double jp);
            Assert.AreEqual(expectedJxx, jxx, 0.0001);
            Assert.AreEqual(expectedJyy, jyy, 0.0001);
            Assert.AreEqual(expectedJxy, jxy, 0.0001);
            Assert.AreEqual(expectedJp, jp, 0.0001);

            Assert.AreEqual(new Point2d(100, 250), SectionHelper.CalculateCentroid(Sx, Sy, section.Area));
            Assert.AreEqual(new Point2d(100, 250), SectionHelper.CalculateCentroid(expectedSxH, expectedSyH, expectedAreaH));

            Assert.AreEqual(ConcreteSectionHelper.GetHomogenizedCentroid(mesh, rebars, material, expectedArea, out double _, out double _), section.Centroid);
            Assert.AreEqual(ConcreteSectionHelper.GetHomogenizedCentroid(phi, mesh, rebars, material, expectedArea, out double _, out double _), section.Centroid);

            Assert.AreEqual(ConcreteSectionHelper.GetHomogenizedCentroid(rebars, material, Sx, Sy, expectedArea, out double _, out double _), section.Centroid);
            Assert.AreEqual(ConcreteSectionHelper.GetHomogenizedCentroid(phi, rebars, material, Sx, Sy, expectedArea, out double _, out double _), section.Centroid);

            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(rebars, section.Centroid, section.Centroid, material, expectedJxx, expectedJyy, expectedJxy, expectedArea,
                out double JxxH, out double JyyH, out double JxyH, out double JpH);

            Assert.AreEqual(expectedJxxH, JxxH, 0.0001);
            Assert.AreEqual(expectedJyyH, JyyH, 0.0001);
            Assert.AreEqual(expectedJxyH, JxyH, 0.0001);
            Assert.AreEqual(expectedJpH, JpH, 0.0001);

            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(phi, material, rebars, section.Centroid, section.Centroid, expectedJxx, expectedJyy, expectedJxy, expectedArea,
                out double JxxH1, out double JyyH1, out double JxyH1, out double JpH1);

            Assert.AreEqual(expectedJxxH1, JxxH1, 0.0001);
            Assert.AreEqual(expectedJyyH1, JyyH1, 0.0001);
            Assert.AreEqual(expectedJxyH1, JxyH1, 0.0001);
            Assert.AreEqual(expectedJpH1, JpH1, 0.0001);
        }

        [TestMethod]
        public void ThinWallTest01()
        {
            var thinWall = new ThinWallSection.ThinWall(10.0, 2.0, 0.0);

            Assert.AreEqual(0, thinWall.CalculateArea() / 20.0 - 1.0, 0.0001);
            Assert.AreEqual(0, thinWall.CalculateJx() / 6.666666667 - 1.0, 0.0001);
            Assert.AreEqual(0, thinWall.CalculateJy() / 166.6666667 - 1.0, 0.0001);
            Assert.AreEqual(0, thinWall.CalculateJxy(), 0.0001);
        }

        [TestMethod]
        public void ThinWallTest02()
        {
            var thinWall = new ThinWallSection.ThinWall(10.0, 2.0, 1.0);

            Assert.AreEqual(0, thinWall.CalculateArea() / 20.0 - 1.0, 0.0001);
            Assert.AreEqual(0, thinWall.CalculateJx() / 119.9584136 - 1.0, 0.0001);
            Assert.AreEqual(0, thinWall.CalculateJy() / 53.37491974 - 1.0, 0.0001);
            Assert.AreEqual(0, thinWall.CalculateJxy() / 72.74379415 - 1.0, 0.0001);
        }

        [TestMethod]
        public void RCAndSteelSection01()
        {
            // Square cross-section with an H-profile inside without fillet radii.
            // Symmetrical shape.
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-400.0, -400.0),
                new Point2d(400.0, -400.0),
                new Point2d(400.0, 400.0),
                new Point2d(-400.0, 400.0)
            }));

            var sectionRC = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            sectionRC.AddSteelSection(
                new SteelSectionPosition(
                    new SteelSection(new SectionH(300.0, 8.5, 290.0, 14.0, 290.0, 14.0, ""), SteelMaterialEN1993Data.S235),
                    Point2d.Origin,
                    0.0,
                    new Vector2d(-290.0 / 2.0, -300.0 / 2.0)
                    )
                );

            // Calculations

            double nHomoTarget = 6.6717909812633494; // 210000 / 31476
            double nHomoCalc = ConcreteSectionHelper.CalculateN(sectionRC.SteelSections[0], sectionRC.ConcreteMaterial);

            double homoAreaTarget = 640000.0 + (nHomoTarget - 1.0) * 10432.0;
            double homoGxTarget = 0.0;
            double homoGyTarget = 0.0;
            double homoJxxTarget = 34133333333.3 + (nHomoTarget - 1.0) * 180430000.0;
            double homoJyyTarget = 34133333333.3 + (nHomoTarget - 1.0) * 56920000.0;
            double homoJxyTarget = 0.0;

            var homo = sectionRC.GetHomogeneizedMechanicalProperties();

            // Test

            Assert.AreEqual(nHomoTarget, nHomoCalc, 0.00001);
            Assert.AreEqual(0.0, homo.areaH / homoAreaTarget - 1.0, 0.000001);
            Assert.AreEqual(homoGxTarget, homo.centroidH.X, 0.000001);
            Assert.AreEqual(homoGyTarget, homo.centroidH.Y, 0.000001);
            Assert.AreEqual(0.0, homo.JxxH / homoJxxTarget - 1.0, 0.000001);
            Assert.AreEqual(0.0, homo.JyyH / homoJyyTarget - 1.0, 0.000001);
            Assert.AreEqual(homoJxyTarget, homo.JxyH, 0.000001);
        }

        [TestMethod]
        public void RCAndSteelSection02()
        {
            // Square cross-section with an H-profile inside without fillet radii.
            // Asymmetrical shape.
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-400.0, -400.0),
                new Point2d(400.0, -400.0),
                new Point2d(400.0, 400.0),
                new Point2d(-400.0, 400.0)
            }));

            var sectionRC = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            double deltaX = 100.0;
            double deltaY = 120.0;

            sectionRC.AddSteelSection(
                new SteelSectionPosition(
                    new SteelSection(new SectionH(300.0, 8.5, 290.0, 14.0, 290.0, 14.0, ""), SteelMaterialEN1993Data.S235),
                    new Point2d(290.0 / 2.0, 300.0 / 2.0),
                    30.0 * Math.PI / 180.0, // 30° --> 0.5235987755983 rad
                    new Vector2d(deltaX - 290.0 / 2.0, deltaY - 300.0 / 2.0)
                    )
                );

            // Calculations

            double nHomoTarget = 6.6717909812633494; // 210000 / 31476
            double nHomoCalc = ConcreteSectionHelper.CalculateN(sectionRC.SteelSections[0], sectionRC.ConcreteMaterial);

            double nHomoNoCls = nHomoTarget - 1.0; // 5.6717909812633494
            double clsArea = 640000.0;
            double clsInertia = 34133333333.3;
            double steelSectionArea = 10432.0;
            double steelJxx = 149554833.741568;
            double steelJyy = 87799510.4250985;
            double steelJxy = -53481981.2655589;

            double homoAreaTarget = clsArea + nHomoNoCls * steelSectionArea;
            double homoGxTarget = nHomoNoCls * steelSectionArea * deltaX / homoAreaTarget;
            double homoGyTarget = nHomoNoCls * steelSectionArea * deltaY / homoAreaTarget;
            double homoJxxTarget = clsInertia + Math.Pow(homoGyTarget, 2.0) * clsArea // cls
                + nHomoNoCls * steelJxx + nHomoNoCls * Math.Pow(deltaY - homoGyTarget, 2.0) * steelSectionArea; // steel
            double homoJyyTarget = clsInertia + Math.Pow(homoGxTarget, 2.0) * clsArea // cls
                + nHomoNoCls * steelJyy + nHomoNoCls * Math.Pow(deltaX - homoGxTarget, 2.0) * steelSectionArea; // steel
            double homoJxyTarget = homoGxTarget * homoGyTarget * clsArea // cls
                + nHomoNoCls * steelJxy + nHomoNoCls * (deltaX - homoGxTarget) * (deltaY - homoGyTarget) * steelSectionArea; // steel

            var homo = sectionRC.GetHomogeneizedMechanicalProperties();

            double homoJ11Target = SectionHelper.CalculateJ11(homoJxxTarget, homoJyyTarget, homoJxyTarget);
            double homoJ22Target = SectionHelper.CalculateJ22(homoJxxTarget, homoJyyTarget, homoJxyTarget);
            double homoAlphaTarget = SectionHelper.CalculateAngle(homoJxxTarget, homoJyyTarget, homoJxyTarget);

            // Test

            Assert.AreEqual(nHomoTarget, nHomoCalc, 0.000001);
            Assert.AreEqual(0.0, homo.areaH / homoAreaTarget - 1.0, 0.0000001);
            Assert.AreEqual(0.0, homo.centroidH.X / homoGxTarget - 1.0, 0.0000001);
            Assert.AreEqual(0.0, homo.centroidH.Y / homoGyTarget - 1.0, 0.0000001);
            Assert.AreEqual(0.0, homo.JxxH / homoJxxTarget - 1.0, 0.0000001);
            Assert.AreEqual(0.0, homo.JyyH / homoJyyTarget - 1.0, 0.0000001);
            Assert.AreEqual(0.0, homo.JxyH / homoJxyTarget - 1.0, 0.000002);

            Assert.AreEqual(0.0, homo.J11H / homoJ11Target - 1.0, 0.0000001);
            Assert.AreEqual(0.0, homo.J22H / homoJ22Target - 1.0, 0.0000001);
            Assert.AreEqual(0.0, homo.angleX / homoAlphaTarget - 1.0, 0.000001);
        }

        [TestMethod]
        public void RCAndSteelSection03()
        {
            /// Square cross-section with four L-profiles inside without fillet radii.
            /// Symmetrical shape.
            ///                 ▲ Y
            ///                 │
            ///                 │
            ///  ┌────────────┬───┬────────────┐
            ///  │ ┌──────────┘   └──────────┐ │
            ///  │ │                         │ │
            ///  │ │                         │ │
            ///  │ │                         │ │
            ///  │ │                         │ │
            ///  ├─┘                         └─┤
            ///  │                             │ ────► X
            ///  ├─┐                         ┌─┤
            ///  │ │                         │ │
            ///  │ │                         │ │
            ///  │ │                         │ │
            ///  │ │                         │ │
            ///  │ └──────────┐   ┌──────────┘ │
            ///  └────────────┴───┴────────────┘
            double delta = 400.0;
            Shape2d shape = new Shape2d(new Polygon2d(new Point2d[]
            {
                new Point2d(-delta, -delta),
                new Point2d(delta, -delta),
                new Point2d(delta, delta),
                new Point2d(-delta, delta)
            }));

            var sectionRC = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C25_30);

            var steelSectionL_A = new SteelSection(new SectionL(250.0, 40.0, 350.0, 40.0, "L300x350x40"), SteelMaterialEN1993Data.S235);
            var steelSectionL_B = new SteelSection(new SectionL(350.0, 40.0, 250.0, 40.0, "L300x350x40"), SteelMaterialEN1993Data.S235);

            sectionRC.AddSteelSection(
                new SteelSectionPosition(
                    steelSectionL_A,
                    Point2d.Origin,
                    0,
                    new Vector2d(-delta, -delta)
                    )
                );
            sectionRC.AddSteelSection(
                new SteelSectionPosition(
                    steelSectionL_A,
                    Point2d.Origin,
                    Math.PI,
                    new Vector2d(delta, delta)
                    )
                );
            sectionRC.AddSteelSection(
                new SteelSectionPosition(
                    steelSectionL_B,
                    Point2d.Origin,
                    0.5 * Math.PI,
                    new Vector2d(delta, -delta)
                    )
                );
            sectionRC.AddSteelSection(
                new SteelSectionPosition(
                    steelSectionL_B,
                    Point2d.Origin,
                    1.5 * Math.PI,
                    new Vector2d(-delta, delta)
                    )
                );

            // Calculations

            double nHomoTarget = 6.6717909812633494; // 210000 / 31476
            double nHomoCalc = ConcreteSectionHelper.CalculateN(sectionRC.SteelSections[0], sectionRC.ConcreteMaterial);

            double nHomoNoCls = nHomoTarget - 1.0; // 5.6717909812633494
            double clsArea = 640000.0;
            double clsInertia = 34133333333.3;
            double steelSectionArea = 22400.0;
            double steelGx = 125.0 - 58.125;
            double steelGy = 175.0 - 58.125;
            double steelJxx = 270167916.666667;
            double steelJyy = 114767916.666667;
            //double steelJxy = -101718750.0;

            double homoAreaTarget = clsArea + 4.0 * nHomoNoCls * steelSectionArea;
            double homoGxTarget = 0.0;
            double homoGyTarget = 0.0;
            double homoJxxTarget = clsInertia // cls
                + 4.0 * nHomoNoCls * steelJxx + 4.0 * nHomoNoCls * Math.Pow(delta - steelGy, 2.0) * steelSectionArea; // steel
            double homoJyyTarget = clsInertia // cls
                + 4.0 * nHomoNoCls * steelJyy + 4.0 * nHomoNoCls * Math.Pow(delta - steelGx, 2.0) * steelSectionArea; // steel
            double homoJxyTarget = 0.0;

            var homo = sectionRC.GetHomogeneizedMechanicalProperties();

            double homoJ11Target = SectionHelper.CalculateJ11(homoJxxTarget, homoJyyTarget, homoJxyTarget);
            double homoJ22Target = SectionHelper.CalculateJ22(homoJxxTarget, homoJyyTarget, homoJxyTarget);
            double homoAlphaTarget = SectionHelper.CalculateAngle(homoJxxTarget, homoJyyTarget, homoJxyTarget);

            // Test

            Assert.AreEqual(nHomoTarget, nHomoCalc, 0.000001);
            Assert.AreEqual(0.0, homo.areaH / homoAreaTarget - 1.0, 0.0000001);
            Assert.AreEqual(homoGxTarget, homo.centroidH.X, 0.0000001);
            Assert.AreEqual(homoGyTarget, homo.centroidH.Y, 0.0000001);
            Assert.AreEqual(0.0, homo.JxxH / homoJxxTarget - 1.0, 0.0000001);
            Assert.AreEqual(0.0, homo.JyyH / homoJyyTarget - 1.0, 0.0000001);
            Assert.AreEqual(homoJxyTarget, homo.JxyH, 0.0000001);

            Assert.AreEqual(0.0, homo.J11H / homoJ11Target - 1.0, 0.0000001);
            Assert.AreEqual(0.0, homo.J22H / homoJ22Target - 1.0, 0.0000001);
            Assert.AreEqual(0.0, homo.angleX / homoAlphaTarget - 1.0, 0.000001);
        }

        #endregion
    }
}