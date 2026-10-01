using System;
using System.Linq;

namespace GPC.Model.Sections.Concrete
{
    /// <summary>
    /// Crack-control data of a reinforced concrete section, with provenance. Explicit data: the exposure class is not inferred and the cover is not
    /// derived from the outline. Lengths mm.
    /// </summary>
    [Serializable]
    public sealed class ConcreteCrackData : IEquatable<ConcreteCrackData>
    {
        /// <summary>Exposure classes accepted (EN 206 / UNI 11104).</summary>
        public static readonly string[] ExposureClasses = { "X0", "XC1", "XC2", "XC3", "XF1", "XC4", "XD1", "XS1", "XA1", "XA2", "XF2", "XF3", "XD2", "XD3", "XS2", "XS3", "XA3", "XF4" };

        private readonly string _exposure, _source;
        private readonly bool _sensitive, _ribbed, _concentricRings;
        private readonly double _cover;
        private readonly double? _maximumBarSpacing;
        /// <summary>Exposure class of the member; null = not given (the check stays insufficient where the standard needs it).</summary>
        public string Exposure => _exposure;
        /// <summary>Reinforcement sensitive to corrosion (NTC 2018 §4.1.2.2.4); ordinary bars are little sensitive.</summary>
        public bool SensitiveReinforcement => _sensitive;
        /// <summary>Cover c to the surface of the longitudinal bars (nominal cover plus link diameter), mm.</summary>
        public double Cover => _cover;
        /// <summary>Ribbed (high-bond) bars; false = plain bars.</summary>
        public bool RibbedBars => _ribbed;
        /// <summary>Circular sections: bars on concentric rings confirmed (the automatic spacing is measured on each ring).</summary>
        public bool ConcentricRings => _concentricRings;
        /// <summary>Maximum spacing of the tensile bars from the drawings, mm; null = measured automatically on the section.</summary>
        public double? MaximumBarSpacing => _maximumBarSpacing;
        public string Source => _source;

        public ConcreteCrackData(string exposure, bool sensitiveReinforcement, double cover, string source, bool ribbedBars = true, bool concentricRings = false,
            double? maximumBarSpacing = null)
        {
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("The provenance of the crack data is required.", nameof(source));
            if (exposure != null && !ExposureClasses.Contains(exposure)) throw new ArgumentException("Unknown exposure class: " + exposure, nameof(exposure));
            if (double.IsNaN(cover) || double.IsInfinity(cover) || cover < 0) throw new ArgumentOutOfRangeException(nameof(cover));
            if (maximumBarSpacing.HasValue && !ConcreteShearReinforcement.Positive(maximumBarSpacing.Value)) throw new ArgumentOutOfRangeException(nameof(maximumBarSpacing));
            _exposure = exposure; _sensitive = sensitiveReinforcement; _cover = cover; _source = source; _ribbed = ribbedBars; _concentricRings = concentricRings;
            _maximumBarSpacing = maximumBarSpacing;
        }

        public bool Equals(ConcreteCrackData o) => o != null && _exposure == o._exposure && _sensitive == o._sensitive && _cover == o._cover && _ribbed == o._ribbed
            && _concentricRings == o._concentricRings && _maximumBarSpacing == o._maximumBarSpacing && _source == o._source;
        public override bool Equals(object obj) => Equals(obj as ConcreteCrackData);
        public override int GetHashCode() => unchecked((_exposure?.GetHashCode() ?? 0) * 31 + _cover.GetHashCode());
    }
}
