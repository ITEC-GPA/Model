using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Concrete
{
    /// <summary>
    /// Detailing data of a reinforced concrete section, with provenance: covers, widths of the tension zones and of the web, aggregate, lap zone
    /// and the designer's confirmations. Explicit data, nothing derived from the outline. Lengths mm. The links are those of
    /// <see cref="ReinforcedConcreteSection.ShearData"/>.
    /// </summary>
    /// <remarks>
    /// Archive contract (5.36.0.0): the data contract repeats the members of the former serializable contract (same names, namespace and order),
    /// so documents written before are read and written again byte for byte. The arrangement of the laps (<see cref="LappedPercent"/>,
    /// <see cref="LapLayers"/>) is optional and written only when given.
    /// </remarks>
    [Serializable]
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.Sections.Concrete")]
    public sealed class ConcreteDetailingData : IEquatable<ConcreteDetailingData>
    {
        [DataMember(Name = "_nominalCover", IsRequired = true)] private double _nominalCover;
        [DataMember(Name = "_coverDeviation", IsRequired = true)] private double _coverDeviation;
        [DataMember(Name = "_topWidth", IsRequired = true)] private double _topWidth;
        [DataMember(Name = "_bottomWidth", IsRequired = true)] private double _bottomWidth;
        [DataMember(Name = "_webWidth", IsRequired = true)] private double _webWidth;
        [DataMember(Name = "_aggregate", IsRequired = true)] private double _aggregate;
        [DataMember(Name = "_minimumDurabilityCover", IsRequired = true)] private double? _minimumDurabilityCover;
        [DataMember(Name = "_lapZone", IsRequired = true)] private bool _lapZone;
        [DataMember(Name = "_compressionBarsRestrained", IsRequired = true)] private bool _compressionBarsRestrained;
        [DataMember(Name = "_endZonesConfirmed", IsRequired = true)] private bool _endZonesConfirmed;
        [DataMember(Name = "_source", IsRequired = true)] private string _source;
        [OptionalField] [DataMember(Name = "_lappedPercent", IsRequired = false, EmitDefaultValue = false)] private double? _lappedPercent;
        [OptionalField] [DataMember(Name = "_lapLayers", IsRequired = false, EmitDefaultValue = false)] private int? _lapLayers;
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
        /// <summary>
        /// Percentage of the tension bars lapped in the section, in [0; 100] (5.36.0.0); null = not given. Read with <see cref="LapLayers"/> at lap
        /// sections (<see cref="LapZone"/>) for EN 1992-1-1 8.7.2(4), as the lap arrangement of the member detailing of GPCChecker.Concrete.
        /// </summary>
        public double? LappedPercent => _lappedPercent;
        /// <summary>Number of layers of the lapped bars, at least 1 (5.36.0.0); null = not given. Given together with <see cref="LappedPercent"/>.</summary>
        public int? LapLayers => _lapLayers;

        public ConcreteDetailingData(double nominalCover, double? minimumDurabilityCover, double coverDeviation, double topWidth, double bottomWidth, double webWidth,
            double aggregate, string source, bool lapZone = false, bool compressionBarsRestrained = false, bool endZonesConfirmed = false)
            : this(nominalCover, minimumDurabilityCover, coverDeviation, topWidth, bottomWidth, webWidth, aggregate, source, lapZone, compressionBarsRestrained,
                endZonesConfirmed, null, null)
        {
        }

        /// <summary>
        /// Detailing data with the arrangement of the laps (5.36.0.0): percentage of the tension bars lapped in the section, in [0; 100], and number of
        /// layers of the lapped bars, at least 1, both given or both null. Every argument is required.
        /// </summary>
        public ConcreteDetailingData(double nominalCover, double? minimumDurabilityCover, double coverDeviation, double topWidth, double bottomWidth, double webWidth,
            double aggregate, string source, bool lapZone, bool compressionBarsRestrained, bool endZonesConfirmed, double? lappedPercent, int? lapLayers)
        {
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("The provenance of the detailing data is required.", nameof(source));
            foreach (var v in new[] { nominalCover, coverDeviation }) if (double.IsNaN(v) || double.IsInfinity(v) || v < 0) throw new ArgumentOutOfRangeException(nameof(nominalCover));
            if (minimumDurabilityCover.HasValue && (double.IsNaN(minimumDurabilityCover.Value) || double.IsInfinity(minimumDurabilityCover.Value) || minimumDurabilityCover < 0))
                throw new ArgumentOutOfRangeException(nameof(minimumDurabilityCover));
            foreach (var v in new[] { topWidth, bottomWidth, webWidth, aggregate }) if (!ConcreteShearReinforcement.Positive(v)) throw new ArgumentOutOfRangeException(nameof(topWidth));
            if (lappedPercent.HasValue != lapLayers.HasValue)
                throw new ArgumentException("Give both the lapped percentage and the number of layers of the lapped bars, or neither.", nameof(lappedPercent));
            if (lappedPercent.HasValue && !(lappedPercent.Value >= 0 && lappedPercent.Value <= 100))
                throw new ArgumentOutOfRangeException(nameof(lappedPercent), "The lapped percentage is in [0; 100].");
            if (lapLayers.HasValue && lapLayers.Value < 1) throw new ArgumentOutOfRangeException(nameof(lapLayers), "At least one layer of lapped bars.");
            _nominalCover = nominalCover; _minimumDurabilityCover = minimumDurabilityCover; _coverDeviation = coverDeviation; _topWidth = topWidth; _bottomWidth = bottomWidth;
            _webWidth = webWidth; _aggregate = aggregate; _source = source; _lapZone = lapZone; _compressionBarsRestrained = compressionBarsRestrained;
            _endZonesConfirmed = endZonesConfirmed; _lappedPercent = lappedPercent; _lapLayers = lapLayers;
        }

        public bool Equals(ConcreteDetailingData o) => o != null && _nominalCover == o._nominalCover && _minimumDurabilityCover == o._minimumDurabilityCover
            && _coverDeviation == o._coverDeviation && _topWidth == o._topWidth && _bottomWidth == o._bottomWidth && _webWidth == o._webWidth && _aggregate == o._aggregate
            && _lapZone == o._lapZone && _compressionBarsRestrained == o._compressionBarsRestrained && _endZonesConfirmed == o._endZonesConfirmed && _source == o._source
            && _lappedPercent == o._lappedPercent && _lapLayers == o._lapLayers;
        public override bool Equals(object obj) => Equals(obj as ConcreteDetailingData);
        public override int GetHashCode() => unchecked(_nominalCover.GetHashCode() * 31 + _topWidth.GetHashCode() * 17 + _webWidth.GetHashCode());
    }
}
