using System;
using GPC.Model.Sections;

namespace GPC.Converter
{
    /// <summary>Forwarder to the shear-area solver of GPCModel, <see cref="GPC.Model.Sections.SectionShearAreas"/> (moved there by review L6,
    /// 4/11/2026, next to the torsion that shares the joining of the regions). Kept only for callers written against the Converter API on the
    /// F6 branches (BridGHe P2B); same values.</summary>
    [Obsolete("Use GPC.Model.Sections.SectionShearAreas.Compute (GPCModel), which also returns the separate bodies and the method.")]
    public static class SectionShearAreas
    {
        /// <summary>Shear areas in the principal 1 and 2 directions (Straus7 ipSA1/ipSA2): <see cref="GPC.Model.Sections.SectionShearAreas.Compute"/>.</summary>
        public static (double Area1, double Area2) Compute(ISectionShape section, double poissonRatio)
        {
            var areas = GPC.Model.Sections.SectionShearAreas.Compute(section, poissonRatio);
            return (areas.Area1, areas.Area2);
        }

        /// <summary>Shear areas and separate bodies: <see cref="GPC.Model.Sections.SectionShearAreas.Compute"/>.</summary>
        public static (double Area1, double Area2) Compute(ISectionShape section, double poissonRatio, out int bodies)
        {
            var areas = GPC.Model.Sections.SectionShearAreas.Compute(section, poissonRatio);
            bodies = areas.Bodies;
            return (areas.Area1, areas.Area2);
        }

        /// <summary>See <see cref="GPC.Model.Sections.SectionShearAreas.AnalysisMesh"/>.</summary>
        public static (double[][] Vertices, int[][] Triangles) AnalysisMesh(ISectionShape section) => GPC.Model.Sections.SectionShearAreas.AnalysisMesh(section);
    }
}
