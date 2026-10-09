using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// An elliptical section, solid or hollow (e.g. the elliptical hollow sections of EN 10210-2): overall width 2a and height 2b, the inner
    /// ellipse of the hollow section with the semi-axes a - t and b - t. The solid ellipse has the exact torsion constant π a³ b³ / (a² + b²),
    /// warping constant π a³ b³ / 24 ((a² - b²) / (a² + b²))² and shear centre in the centre; the hollow one has them from the finite elements
    /// </summary>
    [Serializable]
    public class SectionEllipse : SectionParametric, ISerializable
    {
        private readonly double _width, _height, _thickness;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="width">The overall width (2a)</param>
        /// <param name="height">The overall height (2b)</param>
        /// <param name="thickness">The thickness of the wall (0: solid)</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is not positive or the wall is too thick</exception>
        public SectionEllipse(double width, double height, double thickness = 0.0, string name = "")
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
        protected SectionEllipse(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _width = info.GetDouble("EllipseWidth");
            _height = info.GetDouble("EllipseHeight");
            _thickness = info.GetDouble("EllipseThickness");
        }

        /// <summary>The thickness of the wall (0: solid)</summary>
        public double Thickness => _thickness;

        /// <summary>True if hollow</summary>
        public bool IsHollow => _thickness > 0;

        /// <summary>The exact outer and inner ellipses. Existing numerical properties keep their established discretization.</summary>
        public override System.Collections.Generic.IReadOnlyList<SectionCurveOutline> GetCurveOutlines()
        {
            Curve3d Loop(double inset) => new EllipseCurve3d(new Point3d(_width / 2, _height / 2, 0),
                new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), _width / 2 - inset, _height / 2 - inset);
            return new[] { new SectionCurveOutline(Loop(0), IsHollow ? new[] { Loop(_thickness) } : null) };
        }

        /// <summary>
        /// The ellipse, and the inner one if hollow
        /// </summary>
        /// <returns>The outline</returns>
        protected override Shape2d CreateOutline()
        {
            double a = _width / 2, b = _height / 2;
            return IsHollow ? new Shape2d(Ellipse(a, b, a, b), new[] { Ellipse(a, b, a - _thickness, b - _thickness) }) : new Shape2d(Ellipse(a, b, a, b));
        }

        /// <summary>
        /// The torsion constant: π a³ b³ / (a² + b²) for the solid ellipse; NaN for the hollow one (computed with the finite elements at the first
        /// access)
        /// </summary>
        /// <returns>The torsion constant</returns>
        protected override double CalculateJt()
        {
            if (IsHollow)
                return double.NaN;
            double a = _width / 2, b = _height / 2;
            return Math.PI * Math.Pow(a, 3) * Math.Pow(b, 3) / (a * a + b * b);
        }

        /// <summary>
        /// The warping constant: the integral of the square of the warping function -(a² - b²) / (a² + b²) x y for the solid ellipse; NaN for the
        /// hollow one (computed with the finite elements at the first access)
        /// </summary>
        /// <returns>The warping constant</returns>
        protected override double CalculateJw()
        {
            if (IsHollow)
                return double.NaN;
            double a = _width / 2, b = _height / 2, c = (a * a - b * b) / (a * a + b * b);
            return c * c * Math.PI * Math.Pow(a, 3) * Math.Pow(b, 3) / 24;
        }

        /// <summary>
        /// The shear centre: the centre (also for the hollow ellipse, by the double symmetry)
        /// </summary>
        /// <returns>The centre</returns>
        protected override Point2d CalculateShearCenter() => new Point2d(_width / 2, _height / 2);

        /// <summary>
        /// Solid: all exact; hollow: torsion and warping constants numerical, shear centre exact
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The declared availability</returns>
        protected override PropertyAvailability DeclaredAvailability(SectionProperty property) => IsHollow
            ? Declared(property, PropertyAvailability.Numerical, PropertyAvailability.Numerical, PropertyAvailability.Exact)
            : Declared(property, PropertyAvailability.Exact, PropertyAvailability.Exact, PropertyAvailability.Exact);

        /// <summary>
        /// Symmetric about both the axes
        /// </summary>
        /// <returns>True</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis() => true;

        /// <summary>
        /// Symmetric about both the axes
        /// </summary>
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
            info.AddValue("SectionEllipseVersion", 1);
            info.AddValue("EllipseWidth", _width);
            info.AddValue("EllipseHeight", _height);
            info.AddValue("EllipseThickness", _thickness);
        }

        /// <summary>
        /// The description: "ELL 2a x 2b" or "EHS 2a x 2b x t"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => IsHollow ? $"EHS {_width}x{_height}x{_thickness}" : $"ELL {_width}x{_height}";
    }
}
