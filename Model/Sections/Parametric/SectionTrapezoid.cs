using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A trapezoid (a triangle when the top width is 0, see <see cref="Triangle"/>): bottom and top horizontal, the middle of the top moved by
    /// an offset from the one of the bottom. Torsion constant, warping constant and shear centre from the finite elements
    /// </summary>
    [Serializable]
    public class SectionTrapezoid : SectionParametric, ISerializable
    {
        private readonly double _bottomWidth, _topWidth, _height, _offset;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="bottomWidth">The width of the bottom</param>
        /// <param name="topWidth">The width of the top (0: a triangle)</param>
        /// <param name="height">The height</param>
        /// <param name="topOffset">The horizontal distance of the middle of the top from the one of the bottom (0: isosceles)</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If the bottom width or the height is not positive or the top width is negative</exception>
        public SectionTrapezoid(double bottomWidth, double topWidth, double height, double topOffset = 0.0, string name = "")
            : base(name)
        {
            _bottomWidth = Positive(bottomWidth, nameof(bottomWidth));
            _topWidth = NotNegative(topWidth, nameof(topWidth));
            _height = Positive(height, nameof(height));
            _offset = double.IsNaN(topOffset) || double.IsInfinity(topOffset) ? throw new ArgumentException("The offset must be a number", nameof(topOffset)) : topOffset;
            Build();
        }

        /// <summary>
        /// A triangle with a horizontal base
        /// </summary>
        /// <param name="baseWidth">The width of the base</param>
        /// <param name="height">The height</param>
        /// <param name="apexOffset">The horizontal distance of the apex from the middle of the base (0: isosceles)</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionTrapezoid Triangle(double baseWidth, double height, double apexOffset = 0.0, string name = "") =>
            new SectionTrapezoid(baseWidth, 0.0, height, apexOffset, name);

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionTrapezoid(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _bottomWidth = info.GetDouble("TrapezoidBottomWidth");
            _topWidth = info.GetDouble("TrapezoidTopWidth");
            _height = info.GetDouble("TrapezoidHeight");
            _offset = info.GetDouble("TrapezoidOffset");
        }

        /// <summary>The width of the bottom</summary>
        public double BottomWidth => _bottomWidth;

        /// <summary>The width of the top (0: a triangle)</summary>
        public double TopWidth => _topWidth;

        /// <summary>The horizontal distance of the middle of the top from the one of the bottom</summary>
        public double TopOffset => _offset;

        /// <summary>
        /// The four corners (three for a triangle)
        /// </summary>
        /// <returns>The outline</returns>
        protected override Shape2d CreateOutline()
        {
            double middle = _bottomWidth / 2 + _offset;
            return new Shape2d(Polygon(new[]
            {
                new Point2d(0, 0), new Point2d(_bottomWidth, 0), new Point2d(middle + _topWidth / 2, _height), new Point2d(middle - _topWidth / 2, _height),
            }));
        }

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionTrapezoidVersion", 1);
            info.AddValue("TrapezoidBottomWidth", _bottomWidth);
            info.AddValue("TrapezoidTopWidth", _topWidth);
            info.AddValue("TrapezoidHeight", _height);
            info.AddValue("TrapezoidOffset", _offset);
        }

        /// <summary>
        /// The description: "TRAP bottom / top x height" or "TRI base x height"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => _topWidth > 0 ? $"TRAP {_bottomWidth}/{_topWidth}x{_height}" : $"TRI {_bottomWidth}x{_height}";
    }
}
