using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A steel box girder open at the top ("cassoncino"): a bottom flange, two webs symmetric respect to the vertical axis (vertical or inclined)
    /// and a top flange on each web; the concrete slab closes the box in the composite section. The webs are plates of thickness
    /// <see cref="ThicknessWeb"/> normal to their plane, with their axes at the distance <see cref="WebSpacingTop"/> under the top flanges and
    /// <see cref="WebSpacingBottom"/> on the bottom flange; each top flange is centred on its web. The origin is the bottom left corner of the
    /// bounding box. Area, centroid and moments of inertia are the exact ones of the shape. The torsion constant is the one of the open section
    /// (the closed cell with the slab is not considered)
    /// </summary>
    [Serializable]
    public class SectionSteelBox : ThinWallSection, ISerializable, IEquatable<SectionSteelBox>
    {
        #region Variables

        private double _h, _tw, _btop, _ttop, _bbottom, _tbottom, _spacingTop, _spacingBottom;

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
        public override double Width => Math.Max(_spacingTop + _btop, _bbottom);

        /// <summary>
        /// The thickness of each web, normal to its plane
        /// </summary>
        public double ThicknessWeb => _tw;

        /// <summary>
        /// The width of each top flange
        /// </summary>
        public double LengthTopFlange => _btop;

        /// <summary>
        /// The thickness of the top flanges
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
        /// The distance between the axes of the webs at the top flanges
        /// </summary>
        public double WebSpacingTop => _spacingTop;

        /// <summary>
        /// The distance between the axes of the webs at the bottom flange
        /// </summary>
        public double WebSpacingBottom => _spacingBottom;

        /// <summary>
        /// The vertical height of the webs between the flanges
        /// </summary>
        public double HeightWeb => _h - _ttop - _tbottom;

        /// <summary>
        /// The angle of the webs from the vertical (radians, positive when the box is narrower at the bottom)
        /// </summary>
        public double WebAngle => Math.Atan2((_spacingTop - _spacingBottom) / 2, HeightWeb);

        /// <summary>
        /// The length of each web along its plane between the flanges: Hw / cos α
        /// </summary>
        public double LengthWeb => HeightWeb / Math.Cos(WebAngle);

        /// <summary>
        /// The horizontal width of each web: tw / cos α
        /// </summary>
        public double HorizontalThicknessWeb => _tw / Math.Cos(WebAngle);

        /// <summary>
        /// The clear width of the bottom flange between the webs
        /// </summary>
        public double BottomFlangeInternalWidth => _spacingBottom - HorizontalThicknessWeb;

        /// <summary>
        /// The width of each outstand of the bottom flange beyond the webs (0 if the flange ends at the webs)
        /// </summary>
        public double BottomFlangeOutstand => Math.Max(0, (_bbottom - _spacingBottom - HorizontalThicknessWeb) / 2);

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The overall height</param>
        /// <param name="thicknessWeb">The thickness of each web, normal to its plane</param>
        /// <param name="topFlangeLength">The width of each top flange</param>
        /// <param name="topFlangeThickness">The thickness of the top flanges</param>
        /// <param name="bottomFlangeLength">The width of the bottom flange</param>
        /// <param name="bottomFlangeThickness">The thickness of the bottom flange</param>
        /// <param name="webSpacingTop">The distance between the axes of the webs at the top flanges</param>
        /// <param name="webSpacingBottom">The distance between the axes of the webs at the bottom flange (equal to the top one: vertical webs)</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is not positive, the webs are not on the bottom flange, the top flanges overlap or the
        /// webs are inclined more than 60°</exception>
        public SectionSteelBox(double height, double thicknessWeb, double topFlangeLength, double topFlangeThickness,
            double bottomFlangeLength, double bottomFlangeThickness, double webSpacingTop, double webSpacingBottom, string name = "")
            : base(name)
        {
            _h = Positive(height, nameof(height));
            _tw = Positive(thicknessWeb, nameof(thicknessWeb));
            _btop = Positive(topFlangeLength, nameof(topFlangeLength));
            _ttop = Positive(topFlangeThickness, nameof(topFlangeThickness));
            _bbottom = Positive(bottomFlangeLength, nameof(bottomFlangeLength));
            _tbottom = Positive(bottomFlangeThickness, nameof(bottomFlangeThickness));
            _spacingTop = Positive(webSpacingTop, nameof(webSpacingTop));
            _spacingBottom = Positive(webSpacingBottom, nameof(webSpacingBottom));
            CalculateSection();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="section">The section to copy</param>
        public SectionSteelBox(SectionSteelBox section)
            : this(section._h, section._tw, section._btop, section._ttop, section._bbottom, section._tbottom, section._spacingTop, section._spacingBottom, section.Name)
        {
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionSteelBox(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _h = info.GetDouble("Height");
            _tw = info.GetDouble("ThicknessWeb");
            _btop = info.GetDouble("LengthTopFlange");
            _ttop = info.GetDouble("ThicknessTopFlange");
            _bbottom = info.GetDouble("LengthBottomFlange");
            _tbottom = info.GetDouble("ThicknessBottomFlange");
            _spacingTop = info.GetDouble("WebSpacingTop");
            _spacingBottom = info.GetDouble("WebSpacingBottom");
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
            info.AddValue("SectionSteelBoxVersion", 1);
            info.AddValue("Height", _h);
            info.AddValue("ThicknessWeb", _tw);
            info.AddValue("LengthTopFlange", _btop);
            info.AddValue("ThicknessTopFlange", _ttop);
            info.AddValue("LengthBottomFlange", _bbottom);
            info.AddValue("ThicknessBottomFlange", _tbottom);
            info.AddValue("WebSpacingTop", _spacingTop);
            info.AddValue("WebSpacingBottom", _spacingBottom);
        }

        /// <summary>
        /// The shape of the open box: one polygon, clockwise from the bottom left corner of the bottom flange
        /// </summary>
        /// <returns>The new shape</returns>
        protected override Shape2d GetShape()
        {
            double xc = Width / 2, st = _spacingTop / 2, sb = _spacingBottom / 2, w = HorizontalThicknessWeb / 2, y1 = _tbottom, y2 = _h - _ttop, b = _btop / 2;
            var points = new List<Point2d>();
            void Add(double x, double y)
            {
                if (points.Count == 0 || Math.Abs(points[points.Count - 1].X - (xc + x)) > 1e-12 || Math.Abs(points[points.Count - 1].Y - y) > 1e-12)
                    points.Add(new Point2d(xc + x, y));
            }

            Add(-_bbottom / 2, 0); Add(-_bbottom / 2, y1); Add(-sb - w, y1); Add(-st - w, y2); Add(-st - b, y2); Add(-st - b, _h); Add(-st + b, _h);
            Add(-st + b, y2); Add(-st + w, y2); Add(-sb + w, y1); Add(sb - w, y1); Add(st - w, y2); Add(st - b, y2); Add(st - b, _h); Add(st + b, _h);
            Add(st + b, y2); Add(st + w, y2); Add(sb + w, y1); Add(_bbottom / 2, y1); Add(_bbottom / 2, 0);
            return new Shape2d(new Polygon2d(points.ToArray()));
        }

        /// <summary>
        /// The area of the shape
        /// </summary>
        /// <returns>The exact area</returns>
        protected override double CalculateArea() => Shape.GetArea();

        /// <summary>
        /// The centroid of the shape (on the vertical axis of symmetry)
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
        /// The product of inertia: 0 (symmetric section)
        /// </summary>
        /// <returns>0</returns>
        protected override double CalculateJxy() => 0;

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
        /// The section is always symmetric respect to Y
        /// </summary>
        /// <returns>True</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis() => true;

        /// <summary>
        /// The shear center: on the axis of symmetry, at the height of the centroid (not calculated for the open box)
        /// </summary>
        /// <returns>The point</returns>
        protected override Point2d CalculateShearCenter() => _centroid;

        /// <summary>
        /// The warping constant: not calculated (0)
        /// </summary>
        /// <returns>0</returns>
        protected override double CalculateJw() => 0;

        /// <summary>
        /// Torsion constant of the thin walls of the open box: approximate; warping constant (0) and shear centre (the centroid) are placeholders,
        /// not available: use <see cref="Section.CalculateTorsionProperties"/>
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The declared availability</returns>
        protected override PropertyAvailability DeclaredAvailability(SectionProperty property) =>
            Declared(property, PropertyAvailability.Approximate, PropertyAvailability.NotAvailable, PropertyAvailability.NotAvailable);

        /// <summary>
        /// Builds the thin walls (flanges and webs along their axes), discards the mesh and the shape and calculates the properties
        /// </summary>
        private void CalculateSection()
        {
            if (HeightWeb <= 0)
                throw new ArgumentException("The height must be greater than the sum of the flange thicknesses");
            if (Math.Abs(WebAngle) > Math.PI / 3)
                throw new ArgumentException("The webs cannot be inclined more than 60° from the vertical");
            if (HorizontalThicknessWeb > _btop)
                throw new ArgumentException("The webs cannot be wider than the top flanges");
            if (_spacingTop < _btop)
                throw new ArgumentException("The top flanges cannot overlap: the spacing of the webs at the top must be at least the width of a flange");
            if (BottomFlangeInternalWidth <= 0 || _bbottom < _spacingBottom + HorizontalThicknessWeb - 1e-9)
                throw new ArgumentException("The webs must stand on the bottom flange and be separated");

            double xc = Width / 2, st = _spacingTop / 2, sb = _spacingBottom / 2;
            SetThinWalls(new ThinWall[] {
                new ThinWall(new Point2d(xc - sb, _tbottom), new Point2d(xc - st, _h - _ttop), _tw),
                new ThinWall(new Point2d(xc + sb, _tbottom), new Point2d(xc + st, _h - _ttop), _tw),
                new ThinWall(_btop, _ttop, 0, new Point2d(xc - st, _h - _ttop / 2)),
                new ThinWall(_btop, _ttop, 0, new Point2d(xc + st, _h - _ttop / 2)),
                new ThinWall(_bbottom, _tbottom, 0, new Point2d(xc, _tbottom / 2)) });

            ResetMesh();
            _shape = null;
            SetMechanicalProperties();
        }

        /// <summary>
        /// The description of the section: "BOX h x tw x btop x ttop x bbottom x tbottom / spacing top - spacing bottom"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"BOX {_h}x{_tw}x{_btop}x{_ttop}x{_bbottom}x{_tbottom}/{_spacingTop}-{_spacingBottom}";

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Equality with another section of this type (see <see cref="Equals(SectionSteelBox)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj) => Equals(obj as SectionSteelBox);

        /// <summary>
        /// Equality of the section properties and of the dimensions
        /// </summary>
        /// <param name="other">The section to compare</param>
        /// <returns>True if the sections are equal</returns>
        public bool Equals(SectionSteelBox other) =>
            !(other is null) && base.Equals(other) && _h == other._h && _tw == other._tw && _btop == other._btop && _ttop == other._ttop &&
            _bbottom == other._bbottom && _tbottom == other._tbottom && _spacingTop == other._spacingTop && _spacingBottom == other._spacingBottom;

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
                foreach (double value in new[] { _h, _tw, _btop, _ttop, _bbottom, _tbottom, _spacingTop, _spacingBottom })
                    hashCode = hashCode * -1521134295 + value.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(SectionSteelBox)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(SectionSteelBox left, SectionSteelBox right) => EqualityComparer<SectionSteelBox>.Default.Equals(left, right);

        /// <summary>
        /// Inequality operator (see <see cref="Equals(SectionSteelBox)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(SectionSteelBox left, SectionSteelBox right) => !(left == right);

        #endregion
    }
}
