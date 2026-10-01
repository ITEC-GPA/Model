using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A U (trough) girder, typical of the precast concrete beams: bottom slab, two webs (inclined when the top width differs from the bottom
    /// one) with their thickness normal to their plane, optional lips at the top outside the webs. Open at the top: in a composite deck the slab
    /// closes it (see <see cref="Concrete.ReinforcedConcreteSection.CalculateHomogenizedTorsionProperties"/>). Symmetric about the vertical axis;
    /// torsion constant, warping constant and shear centre of the open section from the finite elements
    /// </summary>
    [Serializable]
    public class SectionUBeam : SectionParametric, ISerializable
    {
        private readonly double _height, _bottomWidth, _topWidth, _bottomThickness, _webThickness, _lipWidth, _lipThickness;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The overall height</param>
        /// <param name="bottomWidth">The outer width at the bottom</param>
        /// <param name="topWidth">The outer width of the webs at the top (without the lips)</param>
        /// <param name="bottomThickness">The thickness of the bottom slab</param>
        /// <param name="webThickness">The thickness of the webs, normal to their plane</param>
        /// <param name="lipWidth">The width of each lip beyond the outer face of its web, at the top (0: no lips)</param>
        /// <param name="lipThickness">The thickness of the lips</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is not positive (negative for the lips), the webs touch or the lips do not fit</exception>
        public SectionUBeam(double height, double bottomWidth, double topWidth, double bottomThickness, double webThickness, double lipWidth = 0.0,
            double lipThickness = 0.0, string name = "")
            : base(name)
        {
            _height = Positive(height, nameof(height));
            _bottomWidth = Positive(bottomWidth, nameof(bottomWidth));
            _topWidth = Positive(topWidth, nameof(topWidth));
            _bottomThickness = Positive(bottomThickness, nameof(bottomThickness));
            _webThickness = Positive(webThickness, nameof(webThickness));
            _lipWidth = NotNegative(lipWidth, nameof(lipWidth));
            _lipThickness = NotNegative(lipThickness, nameof(lipThickness));
            if (_bottomThickness >= _height || (_lipWidth > 0) != (_lipThickness > 0) || _lipThickness >= _height - _bottomThickness)
                throw new ArgumentException("The bottom slab and the lips must fit in the height, the lips need width and thickness");
            Build();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionUBeam(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _height = info.GetDouble("UHeight");
            _bottomWidth = info.GetDouble("UBottomWidth");
            _topWidth = info.GetDouble("UTopWidth");
            _bottomThickness = info.GetDouble("UBottomThickness");
            _webThickness = info.GetDouble("UWebThickness");
            _lipWidth = info.GetDouble("ULipWidth");
            _lipThickness = info.GetDouble("ULipThickness");
        }

        /// <summary>The outer width at the bottom</summary>
        public double BottomWidth => _bottomWidth;

        /// <summary>The outer width of the webs at the top</summary>
        public double TopWidth => _topWidth;

        /// <summary>
        /// The outline, from the bottom left corner
        /// </summary>
        /// <returns>The outline</returns>
        protected override Shape2d CreateOutline()
        {
            double h = _height, dx = (_topWidth - _bottomWidth) / 2;
            double shift = _webThickness * Math.Sqrt(h * h + dx * dx) / h; // horizontal thickness of a web
            double OuterRight(double y) => _bottomWidth / 2 + dx * y / h;
            double InnerRight(double y) => OuterRight(y) - shift;
            if (InnerRight(_bottomThickness) <= 0 || InnerRight(h) <= 0)
                throw new ArgumentException("The webs are too thick for the width");

            var right = new List<Point2d> { new Point2d(_bottomWidth / 2, 0) };
            if (_lipWidth > 0)
            {
                right.Add(new Point2d(OuterRight(h - _lipThickness), h - _lipThickness));
                right.Add(new Point2d(_topWidth / 2 + _lipWidth, h - _lipThickness));
                right.Add(new Point2d(_topWidth / 2 + _lipWidth, h));
            }
            else
            {
                right.Add(new Point2d(_topWidth / 2, h));
            }
            right.Add(new Point2d(InnerRight(h), h));
            right.Add(new Point2d(InnerRight(_bottomThickness), _bottomThickness));

            // the left side mirrored, in the reverse order
            var points = new List<Point2d>(right);
            for (int i = right.Count - 1; i >= 0; i--)
                points.Add(new Point2d(-right[i].X, right[i].Y));
            return new Shape2d(Polygon(points));
        }

        /// <summary>Symmetric about the vertical axis</summary>
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
            info.AddValue("SectionUBeamVersion", 1);
            info.AddValue("UHeight", _height);
            info.AddValue("UBottomWidth", _bottomWidth);
            info.AddValue("UTopWidth", _topWidth);
            info.AddValue("UBottomThickness", _bottomThickness);
            info.AddValue("UWebThickness", _webThickness);
            info.AddValue("ULipWidth", _lipWidth);
            info.AddValue("ULipThickness", _lipThickness);
        }

        /// <summary>
        /// The description: "U h x bottom / top"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"U {_height}x{_bottomWidth}/{_topWidth}";
    }
}
