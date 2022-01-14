using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Materials;
using GPC.Model.Maths.GaussIntegrations;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.TestUtilities;
using GPC.Utilities.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PerformanceTest
{
    [TestClass]
    public class PerformanceSectionTest : UnitTestBase
    {
        [TestMethod]
        public void ReinforcedConcreteSection1()
        {
            ShapeEx shape = new ShapeEx(new Polygon2d(500), ConcreteMaterialEN1992.C25_30, new[] { new Polygon2d(400) });

            ReinforcedConcreteSection section = new ReinforcedConcreteSection(shape);
            section.AddRebar(new ReinforcedConcreteRebar(new RebarSectionCircular(10, RebarMaterial.B450C), new Point2d()));

            Mesh mesh = section.Mesh;
            double Sx = 0;
            double Sy = 0;
            double Jxx = 0;
            double Jyy = 0;
            double Jxy = 0;

            double area = section.Area;

            Action actionStaticMoments = new Action(() =>
            {
                ConcreteSectionHelper.CalculateStaticMoments(mesh, out Sx, out Sy);
            });

            var centroid = SectionHelper.CalculateCentroid(Sx, Sy, area);

            Action actionInertia = new Action(() =>
            {
                ConcreteSectionHelper.CalculateInertiaMoments(mesh, centroid, out Jxx, out Jyy, out Jxy, out double _);
            });

            Action actionInertiaPrincipal = new Action(() =>
            {
                double j11 = SectionHelper.CalculateJ11(Jxx, Jyy, Jxy);
                double j22 = SectionHelper.CalculateJ22(Jxx, Jyy, Jxy);
                double angleX1 = SectionHelper.CalculateAngle(j11, j22, Jxx, Jyy, Jxy); 
            });

            Action actionSection = new Action(() =>
            {
                var _ = new ReinforcedConcreteSection(shape);
            });

            var bb1 = MeasureTime.FunctionExecutionTime(100, actionStaticMoments, true, "StaticMoments");
            var bb2 = MeasureTime.FunctionExecutionTime(100, actionInertia, true, "ActionInertia");
            var bb3 = MeasureTime.FunctionExecutionTime(100, actionInertiaPrincipal, true, "actionInertiaPrincipal");
            var bb4 = MeasureTime.FunctionExecutionTime(100, actionSection, true, "actionSection");

            Console.WriteLine(bb1);
            Console.WriteLine(bb2);
            Console.WriteLine(bb3);
            Console.WriteLine(bb4);
        }
    }
}
