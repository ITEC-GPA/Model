using System;
using System.Collections.Generic;

namespace GPC.Model.Geotechnics.Seepage
{
    /// <summary>Two horizontal soil columns and a vertical impermeable wall of finite thickness at x=0.
    /// Lengths mm, positive elevations upwards. Outer vertical boundaries have fixed reservoir heads;
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
