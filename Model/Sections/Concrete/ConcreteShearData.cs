using System;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Concrete
{
    /// <summary>One layer of shear reinforcement (stirrups or links). Diameter and spacing in mm, inclination in degrees.</summary>
    [Serializable]
    public sealed class ConcreteShearReinforcement : IEquatable<ConcreteShearReinforcement>
    {
        private readonly double _diameter, _spacing, _angle;
        private readonly SteelMaterial _material;
        public double Diameter => _diameter;
        /// <summary>Spacing along the member axis, mm.</summary>
        public double Spacing => _spacing;
        /// <summary>Inclination to the member axis, degrees in [45; 90].</summary>
        public double AngleDegrees => _angle;
        public SteelMaterial Material => _material;
        public double BarArea => Math.PI * _diameter * _diameter / 4;

        public ConcreteShearReinforcement(double diameter, double spacing, SteelMaterial material, double angleDegrees = 90)
        {
            if (!Positive(diameter) || !Positive(spacing)) throw new ArgumentOutOfRangeException(nameof(diameter), "Positive diameter and spacing required.");
            if (double.IsNaN(angleDegrees) || angleDegrees < 45 || angleDegrees > 90) throw new ArgumentOutOfRangeException(nameof(angleDegrees));
            _diameter = diameter; _spacing = spacing; _angle = angleDegrees; _material = material ?? throw new ArgumentNullException(nameof(material));
        }
        public bool Equals(ConcreteShearReinforcement other) => other != null && _diameter == other._diameter && _spacing == other._spacing
            && _angle == other._angle && Equals(_material, other._material);
        public override bool Equals(object obj) => Equals(obj as ConcreteShearReinforcement);
        public override int GetHashCode() => unchecked(_diameter.GetHashCode() * 31 + _spacing.GetHashCode() * 17 + _angle.GetHashCode());
        internal static bool Positive(double v) => !double.IsNaN(v) && !double.IsInfinity(v) && v > 0;
    }

    /// <summary>
    /// Shear-resisting data for the shear force along one section axis (V1 along axis 1, V2 along axis 2). Explicit data: no
    /// value is derived from the outline. Lengths mm, areas mm². Web width is orthogonal to the shear force, depth along it.
    /// </summary>
    [Serializable]
    public sealed class ConcreteShearDirection : IEquatable<ConcreteShearDirection>
    {
        private readonly double _webWidth, _effectiveDepth, _tensionArea, _leverFactor, _axialEccentricity;
        private readonly double? _longitudinalCover;
        private readonly bool _anchored;
        private readonly int _legs;
        /// <summary>bw, mm.</summary>
        public double WebWidth => _webWidth;
        /// <summary>d, mm.</summary>
        public double EffectiveDepth => _effectiveDepth;
        /// <summary>Asl: longitudinal tension reinforcement for the methods without shear reinforcement and Model Code 2010, mm².</summary>
        public double TensionReinforcementArea => _tensionArea;
        /// <summary>Designer confirmation that Asl is anchored beyond the section (required whenever Asl enters the method).</summary>
        public bool TensionReinforcementAnchored => _anchored;
        /// <summary>Effective legs of the shear reinforcement for this direction (0 = none).</summary>
        public int Legs => _legs;
        /// <summary>z/d in (0; 0.9].</summary>
        public double LeverFactor => _leverFactor;
        /// <summary>Distance from the surface to the longitudinal bars cv, mm (DIN lever arm limit); null if not given.</summary>
        public double? LongitudinalCover => _longitudinalCover;
        /// <summary>Δe of Model Code 2010, mm.</summary>
        public double AxialEccentricity => _axialEccentricity;

        public ConcreteShearDirection(double webWidth, double effectiveDepth, double tensionReinforcementArea, bool tensionReinforcementAnchored, int legs,
            double leverFactor = .9, double? longitudinalCover = null, double axialEccentricity = 0)
        {
            if (!ConcreteShearReinforcement.Positive(webWidth) || !ConcreteShearReinforcement.Positive(effectiveDepth)) throw new ArgumentOutOfRangeException(nameof(webWidth));
            if (double.IsNaN(tensionReinforcementArea) || double.IsInfinity(tensionReinforcementArea) || tensionReinforcementArea < 0) throw new ArgumentOutOfRangeException(nameof(tensionReinforcementArea));
            if (legs < 0) throw new ArgumentOutOfRangeException(nameof(legs));
            if (double.IsNaN(leverFactor) || leverFactor <= 0 || leverFactor > .9) throw new ArgumentOutOfRangeException(nameof(leverFactor));
            if (longitudinalCover.HasValue && !ConcreteShearReinforcement.Positive(longitudinalCover.Value)) throw new ArgumentOutOfRangeException(nameof(longitudinalCover));
            if (double.IsNaN(axialEccentricity) || double.IsInfinity(axialEccentricity)) throw new ArgumentOutOfRangeException(nameof(axialEccentricity));
            _webWidth = webWidth; _effectiveDepth = effectiveDepth; _tensionArea = tensionReinforcementArea; _anchored = tensionReinforcementAnchored;
            _legs = legs; _leverFactor = leverFactor; _longitudinalCover = longitudinalCover; _axialEccentricity = axialEccentricity;
        }
        public bool Equals(ConcreteShearDirection o) => o != null && _webWidth == o._webWidth && _effectiveDepth == o._effectiveDepth && _tensionArea == o._tensionArea
            && _anchored == o._anchored && _legs == o._legs && _leverFactor == o._leverFactor && _longitudinalCover == o._longitudinalCover && _axialEccentricity == o._axialEccentricity;
        public override bool Equals(object obj) => Equals(obj as ConcreteShearDirection);
        public override int GetHashCode() => unchecked(_webWidth.GetHashCode() * 31 + _effectiveDepth.GetHashCode() * 17 + _legs);
    }

    /// <summary>Shear data of a reinforced concrete section, with provenance. Null directions are not defined (the check stays Insufficient).</summary>
    [Serializable]
    public sealed class ConcreteShearData : IEquatable<ConcreteShearData>
    {
        private readonly ConcreteShearReinforcement _reinforcement;
        private readonly ConcreteShearDirection _axis1, _axis2;
        private readonly double? _aggregateSize;
        private readonly string _source;
        /// <summary>Null: member without shear reinforcement.</summary>
        public ConcreteShearReinforcement Reinforcement => _reinforcement;
        public ConcreteShearDirection Axis1 => _axis1;
        public ConcreteShearDirection Axis2 => _axis2;
        /// <summary>Maximum aggregate size dg, mm (Model Code 2010 and NS EN 1992-1-1); null if not given.</summary>
        public double? AggregateSize => _aggregateSize;
        public string Source => _source;

        public ConcreteShearData(ConcreteShearReinforcement reinforcement, ConcreteShearDirection axis1, ConcreteShearDirection axis2, string source, double? aggregateSize = null)
        {
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("The provenance of the shear data is required.", nameof(source));
            if (axis1 == null && axis2 == null) throw new ArgumentException("At least one direction is required.");
            if (aggregateSize.HasValue && !ConcreteShearReinforcement.Positive(aggregateSize.Value)) throw new ArgumentOutOfRangeException(nameof(aggregateSize));
            if (reinforcement == null && ((axis1?.Legs ?? 0) != 0 || (axis2?.Legs ?? 0) != 0)) throw new ArgumentException("Legs without shear reinforcement.");
            _reinforcement = reinforcement; _axis1 = axis1; _axis2 = axis2; _source = source; _aggregateSize = aggregateSize;
        }
        public bool Equals(ConcreteShearData o) => o != null && Equals(_reinforcement, o._reinforcement) && Equals(_axis1, o._axis1) && Equals(_axis2, o._axis2)
            && _aggregateSize == o._aggregateSize && _source == o._source;
        public override bool Equals(object obj) => Equals(obj as ConcreteShearData);
        public override int GetHashCode() => unchecked((_axis1?.GetHashCode() ?? 0) * 31 + (_axis2?.GetHashCode() ?? 0));
    }
}
