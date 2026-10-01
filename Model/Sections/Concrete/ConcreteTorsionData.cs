using System;

namespace GPC.Model.Sections.Concrete
{
    /// <summary>
    /// Torsion-resisting data of a reinforced concrete section, with provenance. Explicit data: nothing is derived from the outline.
    /// The closed links are the shear reinforcement of <see cref="ReinforcedConcreteSection.ShearData"/>.
    /// Lengths mm, areas mm².
    /// </summary>
    [Serializable]
    public sealed class ConcreteTorsionData : IEquatable<ConcreteTorsionData>
    {
        private readonly double _enclosedArea, _perimeter, _wallThickness, _longitudinalArea;
        private readonly bool _closedLinksConfirmed, _hollowWithReinforcementOnBothFaces;
        private readonly string _source;
        /// <summary>Ak: area enclosed by the centre line of the walls of the resisting profile, holes included, mm².</summary>
        public double EnclosedArea => _enclosedArea;
        /// <summary>uk: perimeter of Ak, mm.</summary>
        public double Perimeter => _perimeter;
        /// <summary>tef: thickness of the resisting walls, mm.</summary>
        public double WallThickness => _wallThickness;
        /// <summary>ΣAsl: longitudinal bars available for torsion in addition to bending, distributed along the profile, mm².</summary>
        public double LongitudinalArea => _longitudinalArea;
        /// <summary>
        /// Designer confirmation: closed and anchored links, longitudinal bars inside the resisting thickness with a bar in each corner.
        /// Spirals are not equivalent to closed links.
        /// </summary>
        public bool ClosedLinksConfirmed => _closedLinksConfirmed;
        /// <summary>Box section with reinforcement on both faces of the walls (it changes the strut rules of some standards).</summary>
        public bool HollowWithReinforcementOnBothFaces => _hollowWithReinforcementOnBothFaces;
        public string Source => _source;

        public ConcreteTorsionData(double enclosedArea, double perimeter, double wallThickness, double longitudinalArea, bool closedLinksConfirmed, string source,
            bool hollowWithReinforcementOnBothFaces = false)
        {
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("The provenance of the torsion data is required.", nameof(source));
            if (!ConcreteShearReinforcement.Positive(enclosedArea) || !ConcreteShearReinforcement.Positive(perimeter) || !ConcreteShearReinforcement.Positive(wallThickness))
                throw new ArgumentOutOfRangeException(nameof(enclosedArea), "Positive Ak, uk and tef required.");
            if (double.IsNaN(longitudinalArea) || double.IsInfinity(longitudinalArea) || longitudinalArea < 0) throw new ArgumentOutOfRangeException(nameof(longitudinalArea));
            _enclosedArea = enclosedArea; _perimeter = perimeter; _wallThickness = wallThickness; _longitudinalArea = longitudinalArea;
            _closedLinksConfirmed = closedLinksConfirmed; _hollowWithReinforcementOnBothFaces = hollowWithReinforcementOnBothFaces; _source = source;
        }

        public bool Equals(ConcreteTorsionData o) => o != null && _enclosedArea == o._enclosedArea && _perimeter == o._perimeter && _wallThickness == o._wallThickness
            && _longitudinalArea == o._longitudinalArea && _closedLinksConfirmed == o._closedLinksConfirmed
            && _hollowWithReinforcementOnBothFaces == o._hollowWithReinforcementOnBothFaces && _source == o._source;
        public override bool Equals(object obj) => Equals(obj as ConcreteTorsionData);
        public override int GetHashCode() => unchecked(_enclosedArea.GetHashCode() * 31 + _perimeter.GetHashCode() * 17 + _wallThickness.GetHashCode());
    }
}
