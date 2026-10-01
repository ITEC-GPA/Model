using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// An I girder with haunched flanges, typical of the precast and prestressed beams (AASHTO, bulb-T, "trave a I"): top and bottom flanges
    /// with their thickness at the tips and a straight haunch to the web; a T without bottom flange when the bottom flange is as wide as the web
    /// with no thickness, an inverted T without top flange likewise. Symmetric about the vertical axis; torsion constant, warping constant and
    /// shear centre from the finite elements
    /// </summary>
    [Serializable]
    public class SectionHaunchedI : SectionParametric, ISerializable
    {
        private readonly double _height, _webThickness, _topWidth, _topThickness, _topHaunch, _bottomWidth, _bottomThickness, _bottomHaunch;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The overall height</param>
        /// <param name="webThickness">The thickness of the web</param>
        /// <param name="topWidth">The width of the top flange (the web thickness: no top flange)</param>
        /// <param name="topThickness">The thickness of the top flange at its tips</param>
        /// <param name="topHaunch">The height of the haunch between the top flange and the web</param>
        /// <param name="bottomWidth">The width of the bottom flange (the web thickness: no bottom flange)</param>
        /// <param name="bottomThickness">The thickness of the bottom flange at its tips</param>
        /// <param name="bottomHaunch">The height of the haunch between the bottom flange and the web</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is negative, the flanges are narrower than the web or do not fit in the height</exception>
        public SectionHaunchedI(double height, double webThickness, double topWidth, double topThickness, double topHaunch, double bottomWidth,
            double bottomThickness, double bottomHaunch, string name = "")
            : base(name)
        {
            _height = Positive(height, nameof(height));
            _webThickness = Positive(webThickness, nameof(webThickness));
            _topWidth = Positive(topWidth, nameof(topWidth));
            _topThickness = NotNegative(topThickness, nameof(topThickness));
            _topHaunch = NotNegative(topHaunch, nameof(topHaunch));
            _bottomWidth = Positive(bottomWidth, nameof(bottomWidth));
            _bottomThickness = NotNegative(bottomThickness, nameof(bottomThickness));
            _bottomHaunch = NotNegative(bottomHaunch, nameof(bottomHaunch));
            if (_topWidth < _webThickness || _bottomWidth < _webThickness)
                throw new ArgumentException("The flanges cannot be narrower than the web");
            if (_topThickness + _topHaunch + _bottomThickness + _bottomHaunch >= _height)
                throw new ArgumentException("The flanges and the haunches do not fit in the height");
            Build();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionHaunchedI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _height = info.GetDouble("HaunchedIHeight");
            _webThickness = info.GetDouble("HaunchedIWebThickness");
            _topWidth = info.GetDouble("HaunchedITopWidth");
            _topThickness = info.GetDouble("HaunchedITopThickness");
            _topHaunch = info.GetDouble("HaunchedITopHaunch");
            _bottomWidth = info.GetDouble("HaunchedIBottomWidth");
            _bottomThickness = info.GetDouble("HaunchedIBottomThickness");
            _bottomHaunch = info.GetDouble("HaunchedIBottomHaunch");
        }

        /// <summary>The thickness of the web</summary>
        public double WebThickness => _webThickness;

        /// <summary>The width of the top flange</summary>
        public double TopWidth => _topWidth;

        /// <summary>The width of the bottom flange</summary>
        public double BottomWidth => _bottomWidth;

        /// <summary>
        /// The twelve corners (the coincident ones merged)
        /// </summary>
        /// <returns>The outline</returns>
        protected override Shape2d CreateOutline()
        {
            double h = _height, w = _webThickness / 2, bt = _topWidth / 2, bb = _bottomWidth / 2;
            return new Shape2d(Polygon(new[]
            {
                new Point2d(-bb, 0), new Point2d(bb, 0), new Point2d(bb, _bottomThickness), new Point2d(w, _bottomThickness + _bottomHaunch),
                new Point2d(w, h - _topThickness - _topHaunch), new Point2d(bt, h - _topThickness), new Point2d(bt, h), new Point2d(-bt, h),
                new Point2d(-bt, h - _topThickness), new Point2d(-w, h - _topThickness - _topHaunch), new Point2d(-w, _bottomThickness + _bottomHaunch),
                new Point2d(-bb, _bottomThickness),
            }));
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
            info.AddValue("SectionHaunchedIVersion", 1);
            info.AddValue("HaunchedIHeight", _height);
            info.AddValue("HaunchedIWebThickness", _webThickness);
            info.AddValue("HaunchedITopWidth", _topWidth);
            info.AddValue("HaunchedITopThickness", _topThickness);
            info.AddValue("HaunchedITopHaunch", _topHaunch);
            info.AddValue("HaunchedIBottomWidth", _bottomWidth);
            info.AddValue("HaunchedIBottomThickness", _bottomThickness);
            info.AddValue("HaunchedIBottomHaunch", _bottomHaunch);
        }

        /// <summary>
        /// The description: "I h x top width / bottom width"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"I {_height}x{_topWidth}/{_bottomWidth}";
    }
}
