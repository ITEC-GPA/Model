using System;
using System.Collections.Generic;

namespace GPC.Model.Geotechnics.Seepage
{
    /// <summary>Hydraulic condition of an outer vertical boundary of <see cref="EmbeddedWallSeepageDomain"/> below ground.</summary>
    public enum SeepageLateralBoundary
    {
        /// <summary>Fixed reservoir head (LeftHead or RightHead) over the full height below ground.</summary>
        FixedHead,
        /// <summary>Impermeable: no flow across the boundary, e.g. the symmetry axis of a symmetric excavation.</summary>
        NoFlow
    }

    /// <summary>Two horizontal soil columns and a vertical impermeable wall of finite thickness at x=0.
    /// Lengths mm, positive elevations upwards. Each outer vertical boundary has a fixed reservoir head (default) or no flow;
    /// the bottom and wall faces are impermeable. Exposed ground is ponded or an atmospheric seepage face.</summary>
    public sealed class EmbeddedWallSeepageDomain
    {
        public string CaseId { get; set; } = "";
        public double LeftExtent { get; set; }
        public double RightExtent { get; set; }
        public double Bottom { get; set; }
        public double WallTop { get; set; }
        public double WallBottom { get; set; }
        public double WallThickness { get; set; }
        public double LeftGround { get; set; }
        public double RightGround { get; set; }
        public double LeftHead { get; set; }
        public double RightHead { get; set; }
        public double MaximumElementLength { get; set; }
        /// <summary>Condition at x = −LeftExtent below the left ground.</summary>
        public SeepageLateralBoundary LeftBoundary { get; set; } = SeepageLateralBoundary.FixedHead;
        /// <summary>Condition at x = RightExtent below the right ground; NoFlow models the axis of a symmetric excavation of width 2·RightExtent.</summary>
        public SeepageLateralBoundary RightBoundary { get; set; } = SeepageLateralBoundary.FixedHead;
        public IReadOnlyList<SeepageLayer> LeftLayers { get; set; } = Array.Empty<SeepageLayer>();
        public IReadOnlyList<SeepageLayer> RightLayers { get; set; } = Array.Empty<SeepageLayer>();
    }
    public sealed class SeepageLayer
    {
        public double Top { get; }
        public double Bottom { get; }
        public SeepageMaterial Material { get; }
        public SeepageLayer(double top, double bottom, SeepageMaterial material) { Top = top; Bottom = bottom; Material = material; }
    }
}
