using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Sections.Concrete;

namespace GPC.Model.ElementProperties
{
    public static class PlateSections
    {
        internal static void ValidateStorage(AreaElement element)
        {
            if (element.PlateProperty is null) throw new ArgumentException("MissingShellProperty");
            if (element.PlateProperty.PhysicalThickness.HasValue && element.Assignments.PhysicalThickness.HasValue)
                throw new ArgumentException("ConflictingShellThicknessStorage");
            if (element.PlateProperty is ReinforcedConcretePlateSection && element.Assignments.Layers.Count!=0)
                throw new ArgumentException("ConflictingShellReinforcementStorage");
            if (element.Assignments.Layers.Count!=0 && !(element.PlateProperty is ConcretePlateProperty))
                throw new ArgumentException("ConcretePlateRequiredForReinforcement");
        }
        internal static double? PhysicalThickness(AreaElement element)
        { return element.PlateProperty?.PhysicalThickness ?? element.Assignments.PhysicalThickness; }
        internal static IReadOnlyList<ShellRebarLayer> Rebars(AreaElement element)
        { return element.PlateProperty is ReinforcedConcretePlateSection rc ? rc.RebarLayers : element.Assignments.Layers; }
        internal static CoordinateSystem SectionAxes(AreaElement element) => element.Assignments.LayerAxes
            ?? (element.PlateProperty is ReinforcedConcretePlateSection || element.Assignments.Layers.Count==0 ? element.CoordinateSystem : null);
        internal static PlateProperty CopyForChecking(AreaElement element)
        {
            ValidateStorage(element);
            var copy=ModelArchive.CopyValue(element.PlateProperty);
            copy.PhysicalThickness=PhysicalThickness(element);
            if (!(copy is ReinforcedConcretePlateSection) && element.Assignments.Layers.Count!=0)
            {
                var concrete=(ConcretePlateProperty)copy;
                if (!copy.PhysicalThickness.HasValue) throw new ArgumentException("MissingPhysicalThickness");
                copy=new ReinforcedConcretePlateSection(concrete.ConcreteMaterial,copy.PhysicalThickness.Value,
                    concrete.BendingThickness,concrete.MembraneThickness,element.Assignments.Layers.Select(ModelArchive.CopyValue),copy.Name){Id=copy.Id};
            }
            PlateSectionValidation.ValidateProperty(copy);
            return copy;
        }
        /// <summary>Explicitly moves legacy element thickness/rebars into an owned plate property.
        /// Call on an edit-session draft; result/source provenance is not rewritten.</summary>
        public static PlateProperty UpgradeLegacy(AreaElement element)
        {
            if(element==null) throw new ArgumentNullException(nameof(element));
            if(!element.Assignments.PhysicalThickness.HasValue && element.Assignments.Layers.Count==0) return element.PlateProperty;
            var property=CopyForChecking(element);
            element.PlateProperty=property; element.Assignments.PhysicalThickness=null; element.Assignments.Layers.Clear();
            return property;
        }
        // Preserve the physical vocabulary of existing FEM snapshots. Reinforcement is fingerprinted separately.
        internal static PlateProperty AnalysisProperty(AreaElement element)
        {
            if(element.PlateProperty is null) return null;
            if(element.PlateProperty is ReinforcedConcretePlateSection rc)
                return new ConcretePlateProperty(rc.ConcreteMaterial,rc.BendingThickness,rc.MembraneThickness,rc.Name){Id=rc.Id};
            if(!element.PlateProperty.PhysicalThickness.HasValue) return element.PlateProperty;
            var copy=ModelArchive.CopyValue(element.PlateProperty); copy.PhysicalThickness=null; return copy;
        }
    }
}
