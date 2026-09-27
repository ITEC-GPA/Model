using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A welded I/H section with an inclined web: horizontal flanges, each centred on its end of the web, and the web inclined by the horizontal
    /// offset between the end at the bottom flange and the end at the top flange (e.g. the girders of bridges with inclined webs). The web is a
    /// plate of thickness <see cref="ThicknessWeb"/> measured normal to it: between the flanges it is a parallelogram with horizontal width
    /// tw / cos α. The origin is the bottom left corner of the bounding box. Area, centroid and moments of inertia are the exact ones of the shape;
    /// with an offset the section is not symmetric (Jxy ≠ 0, principal axes rotated). No fillets or welds between web and flanges
    /// </summary>
    [Serializable]
    public class SectionHInclinedWeb : ThinWallSection, ISerializable, IEquatable<SectionHInclinedWeb>
    {
        #region Variables

        private double _h, _tw, _btop, _ttop, _bbottom, _tbottom, _offset;

        #endregion

        #region Properties

        /// <summary>
        /// The overall height (the setter calculates the section again)
        /// </summary>
        public override double Height
        {
            get => _h;
            set
            {
                if (_h != value)
                {
                    _h = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The width of the bounding box
        /// </summary>
        public override double Width => Math.Max(TopAxisX + _btop / 2, BottomAxisX + _bbottom / 2) - Math.Min(TopAxisX - _btop / 2, BottomAxisX - _bbottom / 2);

        /// <summary>
        /// The thickness of the web, normal to its plane
        /// </summary>
        public double ThicknessWeb => _tw;

        /// <summary>
        /// The width of the top flange
        /// </summary>
        public double LengthTopFlange => _btop;

        /// <summary>
        /// The thickness of the top flange
        /// </summary>
        public double ThicknessTopFlange => _ttop;

        /// <summary>
        /// The width of the bottom flange
        /// </summary>
        public double LengthBottomFlange => _bbottom;

        /// <summary>
        /// The thickness of the bottom flange
        /// </summary>
        public double ThicknessBottomFlange => _tbottom;

        /// <summary>
        /// The horizontal offset of the axis of the web at the bottom flange respect to the one at the top flange (positive to the right)
        /// </summary>
        public double WebOffset => _offset;

        /// <summary>
        /// The vertical height of the web between the flanges
        /// </summary>
        public double HeightWeb => _h - _ttop - _tbottom;

        /// <summary>
        /// The angle of the web from the vertical (radians, positive when the bottom end is on the right)
        /// </summary>
        public double WebAngle => Math.Atan2(_offset, HeightWeb);

        /// <summary>
        /// The length of the web along its plane between the flanges: Hw / cos α
        /// </summary>
        public double LengthWeb => Math.Sqrt(HeightWeb * HeightWeb + _offset * _offset);

        /// <summary>
        /// The horizontal width of the web: tw / cos α
        /// </summary>
        public double HorizontalThicknessWeb => _tw * LengthWeb / HeightWeb;

        /// <summary>
        /// The x of the axis of the web at the top flange (the axis of the top flange); the left side of the bounding box is at 0
        /// </summary>
        public double TopAxisX => Math.Max(_btop / 2, _bbottom / 2 - _offset);

        /// <summary>
        /// The x of the axis of the web at the bottom flange (the axis of the bottom flange)
        /// </summary>
        public double BottomAxisX => TopAxisX + _offset;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The overall height</param>
        /// <param name="thicknessWeb">The thickness of the web, normal to its plane</param>
        /// <param name="topFlangeLength">The width of the top flange</param>
        /// <param name="topFlangeThickness">The thickness of the top flange</param>
        /// <param name="bottomFlangeLength">The width of the bottom flange</param>
        /// <param name="bottomFlangeThickness">The thickness of the bottom flange</param>
        /// <param name="webOffset">The horizontal offset of the end of the web at the bottom flange respect to the end at the top flange (0: SectionH)</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is not positive, the web height is not positive, the web is wider than a flange or it is
        /// inclined more than 60°</exception>
        public SectionHInclinedWeb(double height, double thicknessWeb, double topFlangeLength, double topFlangeThickness,
            double bottomFlangeLength, double bottomFlangeThickness, double webOffset, string name = "")
            : base(name)
        {
            _h = Positive(height, nameof(height));
            _tw = Positive(thicknessWeb, nameof(thicknessWeb));
            _btop = Positive(topFlangeLength, nameof(topFlangeLength));
            _ttop = Positive(topFlangeThickness, nameof(topFlangeThickness));
            _bbottom = Positive(bottomFlangeLength, nameof(bottomFlangeLength));
            _tbottom = Positive(bottomFlangeThickness, nameof(bottomFlangeThickness));
            _offset = double.IsNaN(webOffset) || double.IsInfinity(webOffset) ? throw new ArgumentException(nameof(webOffset) + " must be finite") : webOffset;
            CalculateSection();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="section">The section to copy</param>
        public SectionHInclinedWeb(SectionHInclinedWeb section)
            : this(section._h, section._tw, section._btop, section._ttop, section._bbottom, section._tbottom, section._offset, section.Name)
        {
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionHInclinedWeb(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _h = info.GetDouble("Height");
            _tw = info.GetDouble("ThicknessWeb");
            _btop = info.GetDouble("LengthTopFlange");
            _ttop = info.GetDouble("ThicknessTopFlange");
            _bbottom = info.GetDouble("LengthBottomFlange");
            _tbottom = info.GetDouble("ThicknessBottomFlange");
            _offset = info.GetDouble("WebOffset");
        }

        private static double Positive(double value, string name) =>
            value > 0 && !double.IsInfinity(value) ? value : throw new ArgumentException(name + " must be positive");

        #endregion

        #region Calculation

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionHInclinedWebVersion", 1);
            info.AddValue("Height", _h);
            info.AddValue("ThicknessWeb", _tw);
            info.AddValue("LengthTopFlange", _btop);
            info.AddValue("ThicknessTopFlange", _ttop);
            info.AddValue("LengthBottomFlange", _bbottom);
            info.AddValue("ThicknessBottomFlange", _tbottom);
            info.AddValue("WebOffset", _offset);
        }

        /// <summary>
        /// The shape of the section: 12 vertices, clockwise from the bottom left corner of the bottom flange as the other H sections
        /// </summary>
        /// <returns>The new shape</returns>
        protected override Shape2d GetShape()
        {
            double xt = TopAxisX, xb = BottomAxisX, w = HorizontalThicknessWeb / 2, y1 = _tbottom, y2 = _h - _ttop;
            var points = new List<Point2d>();
            void Add(double x, double y)
            {
                if (points.Count == 0 || Math.Abs(points[points.Count - 1].X - x) > 1e-12 || Math.Abs(points[points.Count - 1].Y - y) > 1e-12)
                    points.Add(new Point2d(x, y));
            }

            Add(xb - _bbottom / 2, 0); Add(xb - _bbottom / 2, y1); Add(xb - w, y1); Add(xt - w, y2); Add(xt - _btop / 2, y2); Add(xt - _btop / 2, _h);
            Add(xt + _btop / 2, _h); Add(xt + _btop / 2, y2); Add(xt + w, y2); Add(xb + w, y1); Add(xb + _bbottom / 2, y1); Add(xb + _bbottom / 2, 0);
            return new Shape2d(new Polygon2d(points.ToArray()));
        }

        /// <summary>
        /// The area of the shape
        /// </summary>
        /// <returns>The exact area</returns>
        protected override double CalculateArea() => Shape.GetArea();

        /// <summary>
        /// The centroid of the shape
        /// </summary>
        /// <returns>The exact centroid</returns>
        protected override Point2d CalculateCentroid()
        {
            SectionHelper.CalculateStaticMoments(Shape, out double sx, out double sy);
            return SectionHelper.CalculateCentroid(sx, sy, _area);
        }

        /// <summary>
        /// The moment of inertia about the X axis through the centroid, exact on the shape
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJxx()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out double jxx, out _, out _, out _);
            return jxx;
        }

        /// <summary>
        /// The moment of inertia about the Y axis through the centroid, exact on the shape
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJyy()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out _, out double jyy, out _, out _);
            return jyy;
        }

        /// <summary>
        /// The product of inertia through the centroid, exact on the shape (not 0 with an offset)
        /// </summary>
        /// <returns>The product of inertia</returns>
        protected override double CalculateJxy()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out _, out _, out double jxy, out _);
            return jxy;
        }

        /// <summary>
        /// The plastic modulus respect to X: calculated exactly on the shape at the first access
        /// </summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWplX() => double.NaN;

        /// <summary>
        /// The plastic modulus respect to Y: calculated exactly on the shape at the first access
        /// </summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWplY() => double.NaN;

        /// <summary>
        /// The plastic modulus respect to the axis 1: calculated exactly on the shape at the first access
        /// </summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWpl1() => double.NaN;

        /// <summary>
        /// The plastic modulus respect to the axis 2: calculated exactly on the shape at the first access
        /// </summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWpl2() => double.NaN;

        /// <summary>
        /// The vertices of the shape (for the elastic moduli)
        /// </summary>
        /// <returns>The vertices</returns>
        public override Point2d[] GetSectionPoints() => Shape.GetPoints2d();

        /// <summary>
        /// The section is never symmetric respect to X
        /// </summary>
        /// <returns>False</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis() => false;

        /// <summary>
        /// The section is symmetric respect to Y only with the vertical web
        /// </summary>
        /// <returns>True if the offset is 0</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis() => _offset == 0;

        /// <summary>
        /// Calculate the shear center (approximate, as for the H section): on the line between the centroids of the flanges, in the ratio of their
        /// moments of inertia about the vertical axis
        /// </summary>
        /// <returns>The shear center</returns>
        protected override Point2d CalculateShearCenter()
        {
            double top = _ttop * Math.Pow(_btop, 3) / 12, bottom = _tbottom * Math.Pow(_bbottom, 3) / 12, ratio = top / (top + bottom);
            double yTop = _h - _ttop / 2, yBottom = _tbottom / 2;
            return new Point2d(BottomAxisX + (TopAxisX - BottomAxisX) * ratio, yBottom + (yTop - yBottom) * ratio);
        }

        /// <summary>
        /// Calculate the warping constant (approximate, as for the H section, CNR DT 208/2011): d² If,top If,bottom / Iz with d the vertical
        /// distance of the centroids of the flanges
        /// </summary>
        /// <returns>The warping constant</returns>
        protected override double CalculateJw()
        {
            double top = _ttop * Math.Pow(_btop, 3) / 12, bottom = _tbottom * Math.Pow(_bbottom, 3) / 12;
            double d = _h - _ttop / 2 - _tbottom / 2, iz = top + bottom + LengthWeb * Math.Pow(_tw, 3) / 12;
            return d * d * bottom * top / iz;
        }

        /// <summary>
        /// Builds the thin walls (the flanges and the web along its axis between the flanges), discards the mesh and the shape and calculates the
        /// properties
        /// </summary>
        private void CalculateSection()
        {
            if (HeightWeb <= 0)
                throw new ArgumentException("The height must be greater than the sum of the flange thicknesses");
            if (Math.Abs(WebAngle) > Math.PI / 3)
                throw new ArgumentException("The web cannot be inclined more than 60° from the vertical");
            if (HorizontalThicknessWeb > Math.Min(_btop, _bbottom))
                throw new ArgumentException("The web cannot be wider than the flanges it is welded to");

            double xt = TopAxisX, xb = BottomAxisX;
            SetThinWalls(new ThinWall[] {
                new ThinWall(new Point2d(xb, _tbottom), new Point2d(xt, _h - _ttop), _tw),
                new ThinWall(_btop, _ttop, 0, new Point2d(xt, _h - _ttop / 2)),
                new ThinWall(_bbottom, _tbottom, 0, new Point2d(xb, _tbottom / 2)) });

            ResetMesh();
            _shape = null;
            SetMechanicalProperties();
        }

        /// <summary>
        /// The description of the section: "HI h x tw x btop x ttop x bbottom x tbottom / offset"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"HI {_h}x{_tw}x{_btop}x{_ttop}x{_bbottom}x{_tbottom}/{_offset}";

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Equality with another section of this type (see <see cref="Equals(SectionHInclinedWeb)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj) => Equals(obj as SectionHInclinedWeb);

        /// <summary>
        /// Equality of the section properties and of the dimensions
        /// </summary>
        /// <param name="other">The section to compare</param>
        /// <returns>True if the sections are equal</returns>
        public bool Equals(SectionHInclinedWeb other) =>
            !(other is null) && base.Equals(other) && _h == other._h && _tw == other._tw && _btop == other._btop && _ttop == other._ttop &&
            _bbottom == other._bbottom && _tbottom == other._tbottom && _offset == other._offset;

        /// <summary>
        /// The hash code of the section and of the dimensions
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 1348754217;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                foreach (double value in new[] { _h, _tw, _btop, _ttop, _bbottom, _tbottom, _offset })
                    hashCode = hashCode * -1521134295 + value.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(SectionHInclinedWeb)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(SectionHInclinedWeb left, SectionHInclinedWeb right) => EqualityComparer<SectionHInclinedWeb>.Default.Equals(left, right);

        /// <summary>
        /// Inequality operator (see <see cref="Equals(SectionHInclinedWeb)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(SectionHInclinedWeb left, SectionHInclinedWeb right) => !(left == right);

        #endregion
    }
}
