using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Sections.Concrete
{
    /// <summary>
    /// Durability data of a reinforced concrete section, with provenance: exposure classes (EN 206), design working life and the modifiers of
    /// the cover requirement. Explicit data, nothing inferred. The covers, Δcdev and the aggregate are those of
    /// <see cref="ReinforcedConcreteSection.DetailingData"/>. Lengths mm, strengths MPa. Plate elements (NTC) and slab geometry (EN Table 4.3N)
    /// belong to the plate checks: a beam or column section is never a plate.
    /// </summary>
    [Serializable]
    public sealed class ConcreteDurabilityData : IEquatable<ConcreteDurabilityData>
    {
        private readonly string[] _exposures;
        private readonly int _designLife, _abrasion, _ground;
        private readonly bool _strengthReduction, _specialQualityControl, _roughSurface, _ntcQualityReduction;
        private readonly double? _pertinentCmin;
        private readonly string _source;
        /// <summary>Exposure classes acting together (at least one; X0 alone).</summary>
        public IReadOnlyList<string> Exposures => _exposures;
        /// <summary>Design working life, years: 50 or 100.</summary>
        public int DesignLife => _designLife;
        /// <summary>EN Table 4.3N: reduce the structural class by 1 when fck reaches the strength threshold of the exposure.</summary>
        public bool StrengthReduction => _strengthReduction;
        /// <summary>EN Table 4.3N: special quality control of the concrete production (−1 structural class).</summary>
        public bool SpecialQualityControl => _specialQualityControl;
        /// <summary>Uneven surface, for example exposed aggregate: +5 mm (EN 4.4.1.2(11)).</summary>
        public bool RoughSurface => _roughSurface;
        /// <summary>Abrasion addition k1/k2/k3 for XM1/XM2/XM3, mm: 0, 5, 10 or 15 (EN 4.4.1.2(13)).</summary>
        public int Abrasion => _abrasion;
        /// <summary>Concrete cast against prepared ground (40) or directly against soil (75): minimum nominal cover, mm; 0 = formwork.</summary>
        public int Ground => _ground;
        /// <summary>NTC (Circolare C4.1.6.1.3): quality control including the covers, −5 mm.</summary>
        public bool NtcQualityReduction => _ntcQualityReduction;
        /// <summary>NTC: Cmin of the pertinent exposure class, MPa; null = the UNI 11104 class of the exposures.</summary>
        public double? PertinentCmin => _pertinentCmin;
        public string Source => _source;

        public ConcreteDurabilityData(IEnumerable<string> exposures, int designLife, string source, bool strengthReduction = false, bool specialQualityControl = false,
            bool roughSurface = false, int abrasion = 0, int ground = 0, bool ntcQualityReduction = false, double? pertinentCmin = null)
        {
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("The provenance of the durability data is required.", nameof(source));
            var codes = (exposures ?? throw new ArgumentNullException(nameof(exposures))).ToArray();
            if (codes.Length == 0) throw new ArgumentException("Give at least one exposure class.", nameof(exposures));
            foreach (var code in codes) if (!ConcreteCrackData.ExposureClasses.Contains(code)) throw new ArgumentException("Unknown exposure class: " + code, nameof(exposures));
            if (codes.Distinct().Count() != codes.Length) throw new ArgumentException("Repeated exposure class.", nameof(exposures));
            if (codes.Length > 1 && codes.Contains("X0")) throw new ArgumentException("X0 cannot be combined with other exposure classes.", nameof(exposures));
            if (designLife != 50 && designLife != 100) throw new ArgumentOutOfRangeException(nameof(designLife), "Design working life of 50 or 100 years.");
            if (abrasion != 0 && abrasion != 5 && abrasion != 10 && abrasion != 15) throw new ArgumentOutOfRangeException(nameof(abrasion));
            if (ground != 0 && ground != 40 && ground != 75) throw new ArgumentOutOfRangeException(nameof(ground));
            if (pertinentCmin.HasValue && (double.IsNaN(pertinentCmin.Value) || pertinentCmin < 12 || pertinentCmin > 90)) throw new ArgumentOutOfRangeException(nameof(pertinentCmin));
            _exposures = codes; _designLife = designLife; _source = source; _strengthReduction = strengthReduction; _specialQualityControl = specialQualityControl;
            _roughSurface = roughSurface; _abrasion = abrasion; _ground = ground; _ntcQualityReduction = ntcQualityReduction; _pertinentCmin = pertinentCmin;
        }

        public bool Equals(ConcreteDurabilityData o) => o != null && _exposures.SequenceEqual(o._exposures) && _designLife == o._designLife
            && _strengthReduction == o._strengthReduction && _specialQualityControl == o._specialQualityControl && _roughSurface == o._roughSurface
            && _abrasion == o._abrasion && _ground == o._ground && _ntcQualityReduction == o._ntcQualityReduction && _pertinentCmin == o._pertinentCmin && _source == o._source;
        public override bool Equals(object obj) => Equals(obj as ConcreteDurabilityData);
        public override int GetHashCode() => unchecked(string.Join("+", _exposures).GetHashCode() * 31 + _designLife);
    }
}
