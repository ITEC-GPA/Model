using System;
using System.Collections.Generic;

namespace GPC.Model.Geotechnics.EmbeddedWalls
{
    /// <summary>Discrete constructible embedments, all lengths/elevations in mm, z upwards.
    /// Toe = ReferenceElevation - Embedment. The reference is explicitly assigned (normally the lowest excavation level).
    /// Every requested history and criterion must be evaluated; this definition applies no normative factors.</summary>
    public sealed class EmbeddedWallEmbedmentSearch
    {
        public double ReferenceElevation { get; set; }
        public double WallTop { get; set; }
        public double MinimumEmbedment { get; set; }
        public double MaximumEmbedment { get; set; }
        public double Increment { get; set; }
        public string Source { get; set; } = "";
        public IReadOnlyList<EmbeddedWallSearchHistory> Histories { get; set; } = Array.Empty<EmbeddedWallSearchHistory>();
    }

    /// <summary>One independently analysed combination. RequiredCheckIds identify aggregate engineering
    /// criteria for the ENTIRE history (all applicable stages and cuts), not isolated successful checks.</summary>
    public sealed class EmbeddedWallSearchHistory
    {
        public string CaseId { get; set; } = "";
        public IReadOnlyList<string> StageIds { get; set; } = Array.Empty<string>();
        public IReadOnlyList<string> RequiredCheckIds { get; set; } = Array.Empty<string>();
    }
}
