using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A slab on equally spaced webs (ribs), centred in the width: a T (one web), a double T (TT), a ribbed slab or a deck on several cast
    /// girders ("impalcato a più anime"). The webs can be tapered (width under the slab and at the bottom) and have haunches at their joints with
    /// the slab. Symmetric about the vertical axis; torsion constant, warping constant and shear centre from the finite elements; the effective
    /// width of the slab is the one given (the code is not applied)
    /// </summary>
    [Serializable]
    public class SectionMultiWebDeck : SectionParametric, ISerializable
    {
        private readonly double _slabWidth, _slabThickness, _webSpacing, _webTopWidth, _webBottomWidth, _webHeight, _haunchWidth, _haunchHeight;
        private readonly int _webs;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="slabWidth">The width of the slab</param>
        /// <param name="slabThickness">The thickness of the slab</param>
        /// <param name="webs">The number of webs (at least 1)</param>
        /// <param name="webSpacing">The distance between the axes of two webs</param>
        /// <param name="webTopWidth">The width of the webs under the slab</param>
        /// <param name="webBottomWidth">The width of the webs at the bottom</param>
        /// <param name="webHeight">The height of the webs under the slab</param>
        /// <param name="haunchWidth">The horizontal size of the haunches at the joints of the webs with the slab (0: none)</param>
        /// <param name="haunchHeight">The vertical size of the haunches</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is not positive, the webs overlap or are outside the slab</exception>
        public SectionMultiWebDeck(double slabWidth, double slabThickness, int webs, double webSpacing, double webTopWidth, double webBottomWidth,
            double webHeight, double haunchWidth = 0.0, double haunchHeight = 0.0, string name = "")
            : base(name)
        {
            _slabWidth = Positive(slabWidth, nameof(slabWidth));
            _slabThickness = Positive(slabThickness, nameof(slabThickness));
            _webs = webs >= 1 ? webs : throw new ArgumentException("At least one web", nameof(webs));
            _webSpacing = _webs == 1 ? 0.0 : Positive(webSpacing, nameof(webSpacing));
            _webTopWidth = Positive(webTopWidth, nameof(webTopWidth));
            _webBottomWidth = Positive(webBottomWidth, nameof(webBottomWidth));
            _webHeight = Positive(webHeight, nameof(webHeight));
            _haunchWidth = NotNegative(haunchWidth, nameof(haunchWidth));
            _haunchHeight = NotNegative(haunchHeight, nameof(haunchHeight));
            if ((_haunchWidth > 0) != (_haunchHeight > 0) || _haunchHeight >= _webHeight)
                throw new ArgumentException("A haunch needs width and height, lower than the web");
            double half = Math.Max(_webTopWidth / 2 + _haunchWidth, _webBottomWidth / 2);
            if (_webs > 1 && _webSpacing < 2 * half || (_webs - 1) * _webSpacing / 2 + half > _slabWidth / 2 * (1 + 1e-12))
                throw new ArgumentException("The webs overlap or are outside the slab");
            Build();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionMultiWebDeck(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _slabWidth = info.GetDouble("DeckSlabWidth");
            _slabThickness = info.GetDouble("DeckSlabThickness");
            _webs = info.GetInt32("DeckWebs");
            _webSpacing = info.GetDouble("DeckWebSpacing");
            _webTopWidth = info.GetDouble("DeckWebTopWidth");
            _webBottomWidth = info.GetDouble("DeckWebBottomWidth");
            _webHeight = info.GetDouble("DeckWebHeight");
            _haunchWidth = info.GetDouble("DeckHaunchWidth");
            _haunchHeight = info.GetDouble("DeckHaunchHeight");
        }

        /// <summary>The number of webs</summary>
        public int Webs => _webs;

        /// <summary>The width of the slab</summary>
        public double SlabWidth => _slabWidth;

        /// <summary>The thickness of the slab</summary>
        public double SlabThickness => _slabThickness;

        /// <summary>
        /// The bottom of the slab from left to right with the webs, then the top
        /// </summary>
        /// <returns>The outline</returns>
        protected override Shape2d CreateOutline()
        {
            double hw = _webHeight;
            double HalfWidth(double y) => _webBottomWidth / 2 + (_webTopWidth - _webBottomWidth) / 2 * y / hw;
            var points = new List<Point2d> { new Point2d(-_slabWidth / 2, hw) };
            for (int i = 0; i < _webs; i++)
            {
                double x = (i - (_webs - 1) / 2.0) * _webSpacing;
                points.Add(new Point2d(x - HalfWidth(hw) - _haunchWidth, hw));
                points.Add(new Point2d(x - HalfWidth(hw - _haunchHeight), hw - _haunchHeight));
                points.Add(new Point2d(x - HalfWidth(0), 0));
                points.Add(new Point2d(x + HalfWidth(0), 0));
                points.Add(new Point2d(x + HalfWidth(hw - _haunchHeight), hw - _haunchHeight));
                points.Add(new Point2d(x + HalfWidth(hw) + _haunchWidth, hw));
            }
            points.Add(new Point2d(_slabWidth / 2, hw));
            points.Add(new Point2d(_slabWidth / 2, hw + _slabThickness));
            points.Add(new Point2d(-_slabWidth / 2, hw + _slabThickness));
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
            info.AddValue("SectionMultiWebDeckVersion", 1);
            info.AddValue("DeckSlabWidth", _slabWidth);
            info.AddValue("DeckSlabThickness", _slabThickness);
            info.AddValue("DeckWebs", _webs);
            info.AddValue("DeckWebSpacing", _webSpacing);
            info.AddValue("DeckWebTopWidth", _webTopWidth);
            info.AddValue("DeckWebBottomWidth", _webBottomWidth);
            info.AddValue("DeckWebHeight", _webHeight);
            info.AddValue("DeckHaunchWidth", _haunchWidth);
            info.AddValue("DeckHaunchHeight", _haunchHeight);
        }

        /// <summary>
        /// The description: "DECK webs / slab width x total height"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"DECK{_webs} {_slabWidth}x{_webHeight + _slabThickness}";
    }
}
