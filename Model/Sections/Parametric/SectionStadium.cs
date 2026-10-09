using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// An oval section with semicircular ends (stadium), solid or hollow: overall width and height (the ends along the longer one, radius half
    /// the shorter one), the inner oval of the hollow section smaller by the thickness on every side. Shear centre in the centre; torsion and
    /// warping constants from the finite elements
    /// </summary>
    [Serializable]
    public class SectionStadium : SectionParametric, ISerializable
    {
        private readonly double _width, _height, _thickness;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="width">The overall width</param>
        /// <param name="height">The overall height</param>
        /// <param name="thickness">The thickness of the wall (0: solid)</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is not positive or the wall is too thick</exception>
        public SectionStadium(double width, double height, double thickness = 0.0, string name = "")
            : base(name)
        {
            _width = Positive(width, nameof(width));
            _height = Positive(height, nameof(height));
            _thickness = NotNegative(thickness, nameof(thickness));
            if (2 * _thickness >= Math.Min(_width, _height))
                throw new ArgumentException("The wall is too thick", nameof(thickness));
            Build();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionStadium(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _width = info.GetDouble("StadiumWidth");
            _height = info.GetDouble("StadiumHeight");
            _thickness = info.GetDouble("StadiumThickness");
        }

        /// <summary>The thickness of the wall (0: solid)</summary>
        public double Thickness => _thickness;

        /// <summary>True if hollow</summary>
        public bool IsHollow => _thickness > 0;

        /// <summary>Semicircles joined by straight lines, including the inner loop when hollow.</summary>
        public override System.Collections.Generic.IReadOnlyList<SectionCurveOutline> GetCurveOutlines() => new[]
        {
            new SectionCurveOutline(SectionOutline.StadiumCurve(_width / 2, _height / 2, _width, _height),
                IsHollow ? new[] { SectionOutline.StadiumCurve(_width / 2, _height / 2, _width - 2 * _thickness, _height - 2 * _thickness) } : null)
        };

        /// <summary>
        /// The oval, and the inner one if hollow
        /// </summary>
        /// <returns>The outline</returns>
        protected override Shape2d CreateOutline()
        {
            double cx = _width / 2, cy = _height / 2;
            Polygon2d outer = Stadium(cx, cy, _width, _height);
            return IsHollow ? new Shape2d(outer, new[] { Stadium(cx, cy, _width - 2 * _thickness, _height - 2 * _thickness) }) : new Shape2d(outer);
        }

        /// <summary>
        /// The shear centre: the centre (double symmetry)
        /// </summary>
        /// <returns>The centre</returns>
        protected override Point2d CalculateShearCenter() => new Point2d(_width / 2, _height / 2);

        /// <summary>
        /// Torsion and warping constants numerical, shear centre exact
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The declared availability</returns>
        protected override PropertyAvailability DeclaredAvailability(SectionProperty property) =>
            Declared(property, PropertyAvailability.Numerical, PropertyAvailability.Numerical, PropertyAvailability.Exact);

        /// <summary>Symmetric about both the axes</summary>
        /// <returns>True</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis() => true;

        /// <summary>Symmetric about both the axes</summary>
        /// <returns>True</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis() => true;

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionStadiumVersion", 1);
            info.AddValue("StadiumWidth", _width);
            info.AddValue("StadiumHeight", _height);
            info.AddValue("StadiumThickness", _thickness);
        }

        /// <summary>
        /// The description: "OVAL width x height" (x thickness if hollow)
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => IsHollow ? $"OVAL {_width}x{_height}x{_thickness}" : $"OVAL {_width}x{_height}";
    }
}
