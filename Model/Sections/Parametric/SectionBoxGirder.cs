using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A closed box girder, single or multi-cell, rectangular or trapezoidal, of steel or concrete: top slab with cantilevers (thickness
    /// tapered to the tip), bottom slab (optionally beyond the outer webs), webs equally spaced at the top and at the bottom (inclined when the
    /// spacings differ), haunches in the corners of the cells. The axes of the outer webs are at the distance <see cref="TopWebSpacing"/> at the
    /// top fibre and <see cref="BottomWebSpacing"/> at the bottom fibre; the webs have the thickness normal to their plane. Symmetric about the
    /// vertical axis. The closed cells are solved as such by the finite elements (torsion constant, warping constant, shear centre); the
    /// effective widths of the slabs (shear lag) are not applied: they depend on the code
    /// </summary>
    [Serializable]
    public class SectionBoxGirder : SectionParametric, ISerializable
    {
        private readonly double _height, _topWidth, _topThickness, _tipThickness, _bottomThickness, _webThickness, _topSpacing, _bottomSpacing;
        private readonly double _bottomOutstand, _topHaunchWidth, _topHaunchHeight, _bottomHaunchWidth, _bottomHaunchHeight;
        private readonly int _webs;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The overall height</param>
        /// <param name="topWidth">The width of the top slab, cantilevers included</param>
        /// <param name="topThickness">The thickness of the top slab at the outer webs and in the cells</param>
        /// <param name="bottomThickness">The thickness of the bottom slab</param>
        /// <param name="webs">The number of webs (at least 2: cells = webs - 1)</param>
        /// <param name="webThickness">The thickness of the webs, normal to their plane</param>
        /// <param name="topWebSpacing">The distance between the axes of the outer webs at the top fibre</param>
        /// <param name="bottomWebSpacing">The distance between the axes of the outer webs at the bottom fibre (equal: vertical webs)</param>
        /// <param name="tipThickness">The thickness of the cantilevers at their tips (0: <paramref name="topThickness"/>)</param>
        /// <param name="bottomOutstand">The width of the bottom slab beyond the outer face of each outer web</param>
        /// <param name="topHaunchWidth">The horizontal size of the haunches in the top corners of the cells</param>
        /// <param name="topHaunchHeight">The vertical size of the haunches in the top corners of the cells</param>
        /// <param name="bottomHaunchWidth">The horizontal size of the haunches in the bottom corners of the cells</param>
        /// <param name="bottomHaunchHeight">The vertical size of the haunches in the bottom corners of the cells</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is not positive (negative for the optional ones), there are less than 2 webs, the
        /// cells or the cantilevers do not fit</exception>
        public SectionBoxGirder(double height, double topWidth, double topThickness, double bottomThickness, int webs, double webThickness,
            double topWebSpacing, double bottomWebSpacing, double tipThickness = 0.0, double bottomOutstand = 0.0, double topHaunchWidth = 0.0,
            double topHaunchHeight = 0.0, double bottomHaunchWidth = 0.0, double bottomHaunchHeight = 0.0, string name = "")
            : base(name)
        {
            _height = Positive(height, nameof(height));
            _topWidth = Positive(topWidth, nameof(topWidth));
            _topThickness = Positive(topThickness, nameof(topThickness));
            _bottomThickness = Positive(bottomThickness, nameof(bottomThickness));
            _webs = webs >= 2 ? webs : throw new ArgumentException("A box girder has at least 2 webs", nameof(webs));
            _webThickness = Positive(webThickness, nameof(webThickness));
            _topSpacing = Positive(topWebSpacing, nameof(topWebSpacing));
            _bottomSpacing = Positive(bottomWebSpacing, nameof(bottomWebSpacing));
            _tipThickness = tipThickness > 0 ? tipThickness : _topThickness;
            _bottomOutstand = NotNegative(bottomOutstand, nameof(bottomOutstand));
            _topHaunchWidth = NotNegative(topHaunchWidth, nameof(topHaunchWidth));
            _topHaunchHeight = NotNegative(topHaunchHeight, nameof(topHaunchHeight));
            _bottomHaunchWidth = NotNegative(bottomHaunchWidth, nameof(bottomHaunchWidth));
            _bottomHaunchHeight = NotNegative(bottomHaunchHeight, nameof(bottomHaunchHeight));
            Build();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionBoxGirder(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _height = info.GetDouble("BoxHeight");
            _topWidth = info.GetDouble("BoxTopWidth");
            _topThickness = info.GetDouble("BoxTopThickness");
            _tipThickness = info.GetDouble("BoxTipThickness");
            _bottomThickness = info.GetDouble("BoxBottomThickness");
            _webs = info.GetInt32("BoxWebs");
            _webThickness = info.GetDouble("BoxWebThickness");
            _topSpacing = info.GetDouble("BoxTopSpacing");
            _bottomSpacing = info.GetDouble("BoxBottomSpacing");
            _bottomOutstand = info.GetDouble("BoxBottomOutstand");
            _topHaunchWidth = info.GetDouble("BoxTopHaunchWidth");
            _topHaunchHeight = info.GetDouble("BoxTopHaunchHeight");
            _bottomHaunchWidth = info.GetDouble("BoxBottomHaunchWidth");
            _bottomHaunchHeight = info.GetDouble("BoxBottomHaunchHeight");
        }

        /// <summary>The number of webs</summary>
        public int Webs => _webs;

        /// <summary>The number of cells</summary>
        public int Cells => _webs - 1;

        /// <summary>The width of the top slab</summary>
        public double TopWidth => _topWidth;

        /// <summary>The thickness of the top slab at the outer webs and in the cells</summary>
        public double TopThickness => _topThickness;

        /// <summary>The thickness of the cantilevers at their tips</summary>
        public double TipThickness => _tipThickness;

        /// <summary>The thickness of the bottom slab</summary>
        public double BottomThickness => _bottomThickness;

        /// <summary>The thickness of the webs, normal to their plane</summary>
        public double WebThickness => _webThickness;

        /// <summary>The distance between the axes of the outer webs at the top fibre</summary>
        public double TopWebSpacing => _topSpacing;

        /// <summary>The distance between the axes of the outer webs at the bottom fibre</summary>
        public double BottomWebSpacing => _bottomSpacing;

        /// <summary>The width of the bottom slab beyond the outer face of each outer web</summary>
        public double BottomOutstand => _bottomOutstand;

        /// <summary>
        /// The X of the axis of a web at a height (the middle of the section at X = 0)
        /// </summary>
        private double Axis(int web, double y)
        {
            double top = -_topSpacing / 2 + web * _topSpacing / (_webs - 1), bottom = -_bottomSpacing / 2 + web * _bottomSpacing / (_webs - 1);
            return bottom + (top - bottom) * y / _height;
        }

        /// <summary>
        /// The horizontal half thickness of a web: tw / 2 / cos α
        /// </summary>
        private double HalfWidth(int web)
        {
            double dx = Axis(web, _height) - Axis(web, 0);
            return _webThickness / 2 * Math.Sqrt(_height * _height + dx * dx) / _height;
        }

        /// <summary>
        /// The top slab with the cantilevers, the webs and the bottom slab; a hole for each cell
        /// </summary>
        /// <returns>The outline</returns>
        protected override Shape2d CreateOutline()
        {
            double top = _height - _topThickness, bottom = _bottomThickness;
            if (top - bottom <= _topHaunchHeight + _bottomHaunchHeight)
                throw new ArgumentException("The cells do not fit in the height");

            int last = _webs - 1;
            double Right(int web, double y) => Axis(web, y) + HalfWidth(web);
            double Left(int web, double y) => Axis(web, y) - HalfWidth(web);

            double root = Right(last, top);
            if (_topWidth / 2 < root - 1e-9 * _topWidth)
                throw new ArgumentException("The top slab must reach the outer faces of the outer webs");

            var outer = new List<Point2d> { new Point2d(Left(0, 0) - _bottomOutstand, 0), new Point2d(Right(last, 0) + _bottomOutstand, 0) };
            if (_bottomOutstand > 0)
            {
                outer.Add(new Point2d(Right(last, 0) + _bottomOutstand, bottom));
                outer.Add(new Point2d(Right(last, bottom), bottom));
            }
            outer.Add(new Point2d(root, top));
            outer.Add(new Point2d(_topWidth / 2, _height - _tipThickness));
            outer.Add(new Point2d(_topWidth / 2, _height));
            outer.Add(new Point2d(-_topWidth / 2, _height));
            outer.Add(new Point2d(-_topWidth / 2, _height - _tipThickness));
            outer.Add(new Point2d(Left(0, top), top));
            if (_bottomOutstand > 0)
            {
                outer.Add(new Point2d(Left(0, bottom), bottom));
                outer.Add(new Point2d(Left(0, 0) - _bottomOutstand, bottom));
            }

            var cells = new List<Polygon2d>();
            for (int j = 0; j < last; j++)
            {
                double widthBottom = Left(j + 1, bottom) - Right(j, bottom), widthTop = Left(j + 1, top) - Right(j, top);
                if (widthBottom <= 2 * _bottomHaunchWidth || widthTop <= 2 * _topHaunchWidth)
                    throw new ArgumentException("The cells are too narrow for the webs and the haunches");
                cells.Add(Polygon(new[]
                {
                    new Point2d(Right(j, bottom) + _bottomHaunchWidth, bottom), new Point2d(Left(j + 1, bottom) - _bottomHaunchWidth, bottom),
                    new Point2d(Left(j + 1, bottom + _bottomHaunchHeight), bottom + _bottomHaunchHeight),
                    new Point2d(Left(j + 1, top - _topHaunchHeight), top - _topHaunchHeight),
                    new Point2d(Left(j + 1, top) - _topHaunchWidth, top), new Point2d(Right(j, top) + _topHaunchWidth, top),
                    new Point2d(Right(j, top - _topHaunchHeight), top - _topHaunchHeight),
                    new Point2d(Right(j, bottom + _bottomHaunchHeight), bottom + _bottomHaunchHeight),
                }));
            }

            return new Shape2d(Polygon(outer), cells.ToArray());
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
            info.AddValue("SectionBoxGirderVersion", 1);
            info.AddValue("BoxHeight", _height);
            info.AddValue("BoxTopWidth", _topWidth);
            info.AddValue("BoxTopThickness", _topThickness);
            info.AddValue("BoxTipThickness", _tipThickness);
            info.AddValue("BoxBottomThickness", _bottomThickness);
            info.AddValue("BoxWebs", _webs);
            info.AddValue("BoxWebThickness", _webThickness);
            info.AddValue("BoxTopSpacing", _topSpacing);
            info.AddValue("BoxBottomSpacing", _bottomSpacing);
            info.AddValue("BoxBottomOutstand", _bottomOutstand);
            info.AddValue("BoxTopHaunchWidth", _topHaunchWidth);
            info.AddValue("BoxTopHaunchHeight", _topHaunchHeight);
            info.AddValue("BoxBottomHaunchWidth", _bottomHaunchWidth);
            info.AddValue("BoxBottomHaunchHeight", _bottomHaunchHeight);
        }

        /// <summary>
        /// The description: "BOX cells h x top width"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"BOX{Cells} {_height}x{_topWidth}";
    }
}
