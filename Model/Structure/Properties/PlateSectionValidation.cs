using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.PostProcessing;
using GPC.Model.Sections.Concrete;

namespace GPC.Model.ElementProperties
{
    public static class PlateSectionValidation
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
    }
}
