using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Maths.GaussIntegrations;
using GPC.TestUtilities;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Materials;
using GPC.Utilities.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Sections;

namespace PerformanceTest
{
    [TestClass]
    public class PerformanceSectionTest
    {

        [TestMethod]
        public void ReinforcedConcreteSection1()
        {

            ShapeEx shape = new ShapeEx(new Polygon2d(500), ConcreteMaterialEN1992.C25_30, new[] { new Polygon2d(400) });

            Action action = new Action(() =>
            {
                ReinforcedConcreteSection section = new ReinforcedConcreteSection(shape,
                    new ReinforcedConcreteRebar[] { new ReinforcedConcreteRebar(new RebarSectionCircular(10, RebarMaterial.B450C), new Point2d()) });

                var c = section.Centroid;
            });

            ReinforcedConcreteSection section2 = new ReinforcedConcreteSection(shape,
                new ReinforcedConcreteRebar[] { new ReinforcedConcreteRebar(new RebarSectionCircular(10, RebarMaterial.B450C), new Point2d()) });

            

            var bb0 = MeasureTime.FunctionExecutionTime(20, action, true);

            Console.WriteLine(bb0);

        }

    }
}
