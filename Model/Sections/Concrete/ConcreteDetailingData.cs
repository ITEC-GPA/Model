using System;

namespace GPC.Model.Sections.Concrete
{
    /// <summary>
    /// Detailing data of a reinforced concrete section, with provenance: covers, widths of the tension zones and of the web, aggregate, lap zone
    /// and the designer's confirmations. Explicit data, nothing derived from the outline. Lengths mm. The links are those of
    /// <see cref="ReinforcedConcreteSection.ShearData"/>.
    /// </summary>
    [Serializable]
    public sealed class ConcreteDetailingData : IEquatable<ConcreteDetailingData>
    {
        private readonly double _nominalCover, _coverDeviation, _topWidth, _bottomWidth, _webWidth, _aggregate;
        private readonly double? _minimumDurabilityCover;
        private readonly bool _lapZone, _compressionBarsRestrained, _endZonesConfirmed;
        private readonly string _source;
        /// <summary>Nominal cover to the links (to the bars when there are no links), mm.</summary>
        public double NominalCover => _nominalCover;
        /// <summary>cmin,dur of the durability design, mm; null = not given (the cover checks stay pending).</summary>
        public double? MinimumDurabilityCover => _minimumDurabilityCover;
        /// <summary>Allowance for deviation Δcdev, mm.</summary>
        public double CoverDeviation => _coverDeviation;
        /// <summary>bt of the tension zone when the top face is tensioned, mm.</summary>
        public double TopWidth => _topWidth;
        /// <summary>bt of the tension zone when the bottom face is tensioned, mm.</summary>
        public double BottomWidth => _bottomWidth;
        /// <summary>Web width for the minimum links, mm.</summary>
        public double WebWidth => _webWidth;
        /// <summary>Maximum aggregate size dg, mm.</summary>
        public double Aggregate => _aggregate;
        /// <summary>The section lies in a lap zone of the longitudinal bars.</summary>
        public bool LapZone => _lapZone;
        public bool CompressionBarsRestrained => _compressionBarsRestrained;
        /// <summary>End zones confirmed: beams, anchorage of the bottom bars at the end supports and shift rule; columns, link spacing reduced near beams and slabs.</summary>
        public bool EndZonesConfirmed => _endZonesConfirmed;
        public string Source => _source;

        public ConcreteDetailingData(double nominalCover, double? minimumDurabilityCover, double coverDeviation, double topWidth, double bottomWidth, double webWidth,
            double aggregate, string source, bool lapZone = false, bool compressionBarsRestrained = false, bool endZonesConfirmed = false)
        {
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("The provenance of the detailing data is required.", nameof(source));
            foreach (var v in new[] { nominalCover, coverDeviation }) if (double.IsNaN(v) || double.IsInfinity(v) || v < 0) throw new ArgumentOutOfRangeException(nameof(nominalCover));
            if (minimumDurabilityCover.HasValue && (double.IsNaN(minimumDurabilityCover.Value) || double.IsInfinity(minimumDurabilityCover.Value) || minimumDurabilityCover < 0))
                throw new ArgumentOutOfRangeException(nameof(minimumDurabilityCover));
            foreach (var v in new[] { topWidth, bottomWidth, webWidth, aggregate }) if (!ConcreteShearReinforcement.Positive(v)) throw new ArgumentOutOfRangeException(nameof(topWidth));
            _nominalCover = nominalCover; _minimumDurabilityCover = minimumDurabilityCover; _coverDeviation = coverDeviation; _topWidth = topWidth; _bottomWidth = bottomWidth;
            _webWidth = webWidth; _aggregate = aggregate; _source = source; _lapZone = lapZone; _compressionBarsRestrained = compressionBarsRestrained;
            _endZonesConfirmed = endZonesConfirmed;
        }

        public bool Equals(ConcreteDetailingData o) => o != null && _nominalCover == o._nominalCover && _minimumDurabilityCover == o._minimumDurabilityCover
            && _coverDeviation == o._coverDeviation && _topWidth == o._topWidth && _bottomWidth == o._bottomWidth && _webWidth == o._webWidth && _aggregate == o._aggregate
            && _lapZone == o._lapZone && _compressionBarsRestrained == o._compressionBarsRestrained && _endZonesConfirmed == o._endZonesConfirmed && _source == o._source;
        public override bool Equals(object obj) => Equals(obj as ConcreteDetailingData);
        public override int GetHashCode() => unchecked(_nominalCover.GetHashCode() * 31 + _topWidth.GetHashCode() * 17 + _webWidth.GetHashCode());
    }
}
