using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restrains;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;

namespace GPC.Model.PostProcessing
{

    [Serializable]
    public sealed class ShellAssignments
    {
        [OptionalField, GPC.Model.Persistence.FingerprintWhenSet]
        private List<ShellSectionAssignment> _sections;
        /// <summary>Spatial section assignments. When populated, legacy element reinforcement/thickness must be empty.</summary>
        public List<ShellSectionAssignment> Sections => _sections ?? (_sections = new List<ShellSectionAssignment>());
        internal IReadOnlyList<ShellSectionAssignment> SectionItems => _sections ?? (IReadOnlyList<ShellSectionAssignment>)Array.Empty<ShellSectionAssignment>();
        public ShellSectionAssignment SectionAt(Point3d point, string boundarySectionId = null)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            NumericGuard.Finite(point.X, "point x"); NumericGuard.Finite(point.Y, "point y"); NumericGuard.Finite(point.Z, "point z");
            ShellSectionValidation.ValidateAssignments(SectionItems);
            var matches = SectionItems.Where(s => s.Locate(point) != ShellRegionLocation.Outside).ToArray();
            if (matches.Length == 0) throw new InvalidOperationException("MissingShellSectionAtPoint");
            if (matches.Length == 1 && (boundarySectionId == null || matches[0].Id == boundarySectionId)) return matches[0];
            if (boundarySectionId != null && matches.All(s => s.Locate(point) == ShellRegionLocation.Boundary))
            {
                var selected = matches.SingleOrDefault(s => s.Id == boundarySectionId);
                if (selected != null) return selected;
            }
            throw new InvalidOperationException("AmbiguousShellSectionAtPoint");
        }
        /// <summary>Legacy storage; new models put thickness in PlateProperty.</summary>
        public double? PhysicalThickness { get; set; }
        public double? Offset { get; set; }
        public CoordinateSystem LayerAxes { get; set; }
        public List<ShellRebarLayer> Layers { get; private set; } = new List<ShellRebarLayer>();
        public string ReinforcementZone { get; set; }
    }
}
