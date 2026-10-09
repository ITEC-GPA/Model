using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.ElementProperties;
using GPC.Model.Sections.Concrete;

namespace GPC.Model.PostProcessing
{
    public static class ShellSectionValidation
    {
        public static void ValidateProperty(PlateProperty property)
        {
            if (property is null) throw new ArgumentException("MissingShellSectionProperty");
            if (property.PhysicalThickness.HasValue && (!(NumericGuard.Finite(property.PhysicalThickness.Value,"thickness")>0)))
                throw new ArgumentException("InvalidPhysicalThickness");
            if (property is IFemPlateProperty fem && (!(NumericGuard.Finite(fem.BendingThickness,"bending thickness")>0)
                || !(NumericGuard.Finite(fem.MembraneThickness,"membrane thickness")>0) || fem.Material==null)) throw new ArgumentException("InvalidShellFemProperty");
            if (property is ReinforcedConcretePlateSection concrete)
            {
                if (!property.PhysicalThickness.HasValue) throw new ArgumentException("MissingPhysicalThickness");
                foreach(var layer in concrete.RebarLayers)
                    if (layer==null || layer.Steel==null || string.IsNullOrWhiteSpace(layer.PhysicalFace)
                        || !(NumericGuard.Finite(layer.Diameter,"diameter")>0) || !(NumericGuard.Finite(layer.Pitch,"pitch")>0)
                        || Math.Abs(NumericGuard.Finite(layer.AxisPositionThroughThickness,"layer depth"))+layer.Diameter/2>property.PhysicalThickness.Value/2
                        || double.IsNaN(layer.DirectionRadians) || double.IsInfinity(layer.DirectionRadians)) throw new ArgumentException("InvalidShellRebarLayer");
            }
        }
        public static void ValidateAssignments(IReadOnlyList<ShellSectionAssignment> sections)
        {
            if(sections==null || sections.Any(s=>s==null || string.IsNullOrWhiteSpace(s.Id)) || sections.Select(s=>s.Id).Distinct(StringComparer.Ordinal).Count()!=sections.Count)
                throw new ArgumentException("UniqueShellSectionIdsRequired");
            foreach(var section in sections) { Axes.Validate(section.Axes); ValidateProperty(section.Property); }
            for(int i=0;i<sections.Count;i++) for(int j=i+1;j<sections.Count;j++)
            {
                var a=sections[i]; var b=sections[j];
                if(Math.Abs(Axes.Dot(a.Axes.V3,b.Axes.V3))<1-1e-8 || Math.Abs(Axes.Dot(b.Axes.Origin-a.Axes.Origin,a.Axes.V3))>ShellSectionRegion.Tolerance)
                    throw new ArgumentException("ShellSectionPlaneMismatch");
                if(a.Region==null || b.Region==null || ShellSectionRegion.Overlap(a.Region.Vertices.ToArray(),b.GlobalVertices().Select(a.Project).ToArray()))
                    throw new ArgumentException("OverlappingShellSectionRegions");
            }
        }
    }
}
