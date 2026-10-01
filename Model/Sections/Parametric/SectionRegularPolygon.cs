using GPC.Geometry;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A regular polygon, solid or hollow (e.g. the polygonal poles): the number of sides and the circumscribed diameter (see
    /// <see cref="FromAcrossFlats"/>), a side at the bottom; the inner polygon of the hollow section has the sides at the distance of the thickness.
    /// The shear centre is the centre (symmetry of rotation); torsion and warping constants from the finite elements
    /// </summary>
    [Serializable]
    public class SectionRegularPolygon : SectionParametric, ISerializable
    {
        private readonly int _sides;
        private readonly double _diameter, _thickness;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="sides">The number of sides (at least 3)</param>
        /// <param name="circumscribedDiameter">The diameter of the circle through the corners</param>
        /// <param name="thickness">The thickness of the wall (0: solid)</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If there are less than 3 sides, the diameter is not positive or the wall is too thick</exception>
        public SectionRegularPolygon(int sides, double circumscribedDiameter, double thickness = 0.0, string name = "")
            : base(name)
        {
            _sides = sides >= 3 ? sides : throw new ArgumentException("A polygon has at least 3 sides", nameof(sides));
            _diameter = Positive(circumscribedDiameter, nameof(circumscribedDiameter));
            _thickness = NotNegative(thickness, nameof(thickness));
            if (_thickness >= Apothem)
                throw new ArgumentException("The wall is too thick", nameof(thickness));
            Build();
        }

        /// <summary>
        /// A regular polygon from the distance between two opposite sides (for an odd number of sides, twice the apothem)
        /// </summary>
        /// <param name="sides">The number of sides (at least 3)</param>
        /// <param name="acrossFlats">The distance between two opposite sides (twice the distance of the sides from the centre)</param>
        /// <param name="thickness">The thickness of the wall (0: solid)</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionRegularPolygon FromAcrossFlats(int sides, double acrossFlats, double thickness = 0.0, string name = "") =>
            new SectionRegularPolygon(sides, acrossFlats / Math.Cos(Math.PI / Math.Max(3, sides)), thickness, name);

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionRegularPolygon(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _sides = info.GetInt32("PolygonSides");
            _diameter = info.GetDouble("PolygonDiameter");
            _thickness = info.GetDouble("PolygonThickness");
        }

        /// <summary>The number of sides</summary>
        public int Sides => _sides;

        /// <summary>The diameter of the circle through the corners</summary>
        public double CircumscribedDiameter => _diameter;

        /// <summary>The distance of the sides from the centre</summary>
        public double Apothem => _diameter / 2 * Math.Cos(Math.PI / _sides);

        /// <summary>The thickness of the wall (0: solid)</summary>
        public double Thickness => _thickness;

        /// <summary>True if hollow</summary>
        public bool IsHollow => _thickness > 0;

        /// <summary>
        /// The polygon with a side at the bottom, and the inner one if hollow
        /// </summary>
        /// <returns>The outline</returns>
        protected override Shape2d CreateOutline()
        {
            Polygon2d Regular(double radius) => new Polygon2d(Enumerable.Range(0, _sides).Select(k =>
            {
                double angle = -Math.PI / 2 - Math.PI / _sides + 2 * Math.PI * k / _sides;
                return new Point2d(radius * Math.Cos(angle), radius * Math.Sin(angle));
            }).ToArray());

            Polygon2d outer = Regular(_diameter / 2);
            return IsHollow ? new Shape2d(outer, new[] { Regular(_diameter / 2 - _thickness / Math.Cos(Math.PI / _sides)) }) : new Shape2d(outer);
        }

        /// <summary>
        /// The shear centre: the centre (symmetry of rotation)
        /// </summary>
        /// <returns>The centre</returns>
        protected override Point2d CalculateShearCenter() => _centroid;

        /// <summary>
        /// Torsion and warping constants numerical, shear centre exact
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The declared availability</returns>
        protected override PropertyAvailability DeclaredAvailability(SectionProperty property) =>
            Declared(property, PropertyAvailability.Numerical, PropertyAvailability.Numerical, PropertyAvailability.Exact);

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionRegularPolygonVersion", 1);
            info.AddValue("PolygonSides", _sides);
            info.AddValue("PolygonDiameter", _diameter);
            info.AddValue("PolygonThickness", _thickness);
        }

        /// <summary>
        /// The description: "POLY sides / diameter" (x thickness if hollow)
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => IsHollow ? $"POLY{_sides} {_diameter}x{_thickness}" : $"POLY{_sides} {_diameter}";
    }
}
