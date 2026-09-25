using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A channel (C) section: a web on the left (x from 0 to the web thickness) and two flanges towards the positive X, with different
    /// lengths and thicknesses; the origin is the bottom left corner
    /// </summary>
    [Serializable]
    public class SectionC : ThinWallSection, ISerializable, IEquatable<SectionC>
    {
        #region Variables

        /// <summary>
        /// The height
        /// </summary>
        protected double _h;
        /// <summary>
        /// The thickness of the web
        /// </summary>
        protected double _tw;
        /// <summary>
        /// The length of the bottom flange (from the outer side of the web)
        /// </summary>
        protected double _lengthBottom;
        /// <summary>
        /// The thickness of the bottom flange
        /// </summary>
        protected double _tBottom;
        /// <summary>
        /// The length of the top flange (from the outer side of the web)
        /// </summary>
        protected double _lengthTop;
        /// <summary>
        /// The thickness of the top flange
        /// </summary>
        protected double _tTop;

        /// <summary>
        /// Inner radius of curvature or throat height
        /// </summary>
        private readonly double _r1;
        /// <summary>
        /// Outer radius of curvature
        /// </summary>
        private readonly double _r2;

        #endregion

        #region Properties

        /// <summary>
        /// The height (the setter calculates the section again)
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
        /// The maximum length of the flanges
        /// </summary>
        public override double Width => Math.Max(_lengthBottom, _lengthTop);

        /// <summary>
        /// The height of the web between the flanges
        /// </summary>
        public double HeightWeb => _h - _tBottom - _tTop;

        /// <summary>
        /// The thickness of the web (the setter calculates the section again)
        /// </summary>
        public double ThicknessWeb
        {
            get => _tw;
            set
            {
                if (_tw != value)
                {
                    _tw = value;
                    CalculateSection();
                }
            }
        }
        /// <summary>
        /// The length of the bottom flange (the setter calculates the section again)
        /// </summary>
        public double LengthBottom
        {
            get => _lengthBottom;
            set

            {
                if (_lengthBottom != value)
                {
                    _lengthBottom = value;
                    CalculateSection();
                }
            }
        }
        /// <summary>
        /// The thickness of the bottom flange (the setter calculates the section again)
        /// </summary>
        public double ThicknessBottom
        {
            get => _tBottom;
            set

            {
                if (_tBottom != value)
                {
                    _tBottom = value;
                    CalculateSection();
                }
            }
        }
        /// <summary>
        /// The length of the top flange (the setter calculates the section again)
        /// </summary>
        public double LengthTop
        {
            get => _lengthTop;
            set

            {
                if (_lengthTop != value)
                {
                    _lengthTop = value;
                    CalculateSection();
                }
            }
        }
        /// <summary>
        /// The thickness of the top flange (the setter calculates the section again)
        /// </summary>
        public double ThicknessTop
        {
            get => _tTop;
            set

            {
                if (_tTop != value)
                {
                    _tTop = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
		/// Inner radius of curvature or throat height.
		/// </summary>
        public double R1 => _r1;

        /// <summary>
		/// Outer radius of curvature.
		/// </summary>
        public double R2 => _r2;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The height</param>
        /// <param name="thicknessWeb">The thickness of the web</param>
        /// <param name="lengthTop">The length of the top flange</param>
        /// <param name="thicknessTop">The thickness of the top flange</param>
        /// <param name="lengthBottom">The length of the bottom flange</param>
        /// <param name="thicknessBottom">The thickness of the bottom flange</param>
        /// <param name="name">The name</param>
        /// <param name="radiusInternal">Inner radius of curvature or throat height (negative: 0)</param>
        /// <param name="radiusExternal">Outer radius of curvature (negative: 0)</param>
        /// <exception cref="ArgumentException">If a dimension is negative</exception>
        public SectionC(double height, double thicknessWeb, double lengthTop, double thicknessTop,
            double lengthBottom, double thicknessBottom, string name = "",
            double radiusInternal = 0, double radiusExternal = 0)
                    : base(name)
        {
            _h = height < 0 ? throw new ArgumentException($"height cannot be lower than zero") : height;
            _lengthTop = lengthTop < 0 ? throw new ArgumentException($"Top lenght cannot be lower than zero") : lengthTop;
            _lengthBottom = lengthBottom < 0 ? throw new ArgumentException($"Bottom lenght cannot be lower than zero") : lengthBottom;
            _tBottom = thicknessBottom < 0 ? throw new ArgumentException($"Bottom thickness cannot be lower than zero") : thicknessBottom;
            _tTop = thicknessTop < 0 ? throw new ArgumentException($"Top thickness cannot be lower than zero") : thicknessTop;
            _tw = thicknessWeb < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : thicknessWeb;

            _r1 = radiusInternal < 0 ? 0 : radiusInternal;
            _r2 = radiusExternal < 0 ? 0 : radiusExternal;

            CalculateSection();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionC(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionCVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _h = info.GetDouble("Height");
            _tw = info.GetDouble("ThicknessWeb");
            _lengthBottom = info.GetDouble("LengthBottom");
            _tBottom = info.GetDouble("ThicknessBottom");
            _lengthTop = info.GetDouble("LengthTop");
            _tTop = info.GetDouble("ThicknessTop");
            _r1 = info.GetDouble("R1");
            _r2 = info.GetDouble("R2");
        }

        #endregion

        #region Public override method

        /// <summary>
        /// The shape of the section (without radii)
        /// </summary>
        /// <returns>The new shape</returns>
        protected override Shape2d GetShape()
        {
            return new Shape2d(new Polygon2d(new Point2d[] {
                    new Point2d(0.0, 0.0),
                    new Point2d(0.0, _h),
                    new Point2d(_lengthTop, _h),
                    new Point2d(_lengthTop, _h - _tTop),
                    new Point2d(_tw, _h - _tTop),
                    new Point2d(_tw, _tBottom),
                    new Point2d(_lengthBottom, _tBottom),
                    new Point2d(_lengthBottom, 0.0) }));
        }

        /// <summary>
        /// Calculate the warping constant (CNR DT 208/2001, with the bottom flange)
        /// </summary>
        /// <returns>The warping constant</returns>
        protected override double CalculateJw()
        {
            //CNR DT 208/2001
            double hf = _h - _tTop / 2.0 - _tBottom / 2.0;
            double length = _lengthBottom - _tw / 2.0;
            return hf * hf * Math.Pow(length, 3.0) * _tBottom / 12.0 * (2.0 * hf * _tw + 3.0 * length * _tBottom) / (hf * ThicknessWeb + 6.0 * length * _tBottom);
        }

        /// <summary>
        /// Calculate the torsion constant: sum of b t³ / 3 of the walls (middle line lengths)
        /// </summary>
        /// <returns>The torsion constant</returns>
        protected override double CalculateJt()
        {
            return 1.0 / 3.0 * (_lengthTop - _tw / 2.0) * Math.Pow(_tTop, 3.0) + 1.0 / 3.0 * (_h - _tTop / 2.0 - _tBottom / 2.0) *
                Math.Pow(_tw, 3.0) + 1.0 / 3.0 * (_lengthBottom - _tw / 2.0) * Math.Pow(_tBottom, 3.0);
        }

        /// <summary>
        /// Calculate the shear center (CNR DT 208/2001, with the bottom flange): on the left of the web, at the height of the centroid
        /// </summary>
        /// <returns>The shear center</returns>
        protected override Point2d CalculateShearCenter()
        {
            //CNR DT 208/2001
            double hf = _h - _tTop / 2.0 - _tBottom / 2.0;
            double length = _lengthBottom - _tw / 2.0;
            double tf = _tBottom;
            return new Point2d(_tw / 2.0 - 3.0 * length * length * tf / (hf * _tw + 6.0 * length * tf), CalculateCentroid().Y);
        }

        /// <summary>
        /// Calculate the plastic modulus respect to Y: closed form for the section symmetric respect to X, otherwise the one of the base class
        /// </summary>
        /// <returns>The plastic modulus respect to Y</returns>
        protected override double CalculateWplY()
        {
            if (IsSymmetricAlongXLocalAxis)
            {
                if (_area / 2.0 >= _h * _tw)
                {
                    double hDown = Area / 2.0 / (_tTop + _tBottom);
                    var secTop = new SectionT(_lengthBottom - hDown, _h, _tBottom + _tTop, _tw, string.Empty);
                    return Area / 2.0 * (hDown / 2.0 + secTop.DistanceYCentroidFromBottom());
                }
                else
                {
                    double tEff = Area / 2.0 / Height;        // rettangolo alto H e spesso tEff
                    var sectionC = new SectionC(Height, ThicknessWeb - tEff, LengthTop, ThicknessTop, LengthBottom, ThicknessBottom, string.Empty);
                    return Area / 2.0 * (tEff / 2 + sectionC.DistanceXCentroidFromLeft());
                }
            }
            else
                return base.CalculateWplY();
        }

        /// <summary>
        /// Calculate the plastic modulus respect to X: closed form for the section symmetric respect to X with the neutral axis in the web,
        /// otherwise the one of the base class
        /// </summary>
        /// <returns>The plastic modulus respect to X</returns>
        protected override double CalculateWplX()
        {
            if (IsSymmetricAlongXLocalAxis)
            {
                if (_area / 2.0 >= _tTop * _lengthTop)
                {
                    double hTop = _tTop + (_area / 2.0 - _tTop * _lengthTop) / _tw;
                    var secTop = new SectionT(hTop, _lengthTop, _tw, _tTop, string.Empty);
                    var secBottom = new SectionT(_h - hTop, _lengthBottom, _tw, _tBottom, string.Empty);
                    return _area / 2.0 * (secTop.DistanceYCentroidFromBottom() + secBottom.DistanceYCentroidFromBottom());
                }
                else
                    return base.CalculateWplX(); // plastic neutral axis in the flange: the modulus of ThinWallSection (before, an exception in the constructor)
            }
            else
                return base.CalculateWplX();
        }

        // The moduli respect to X and Y (the principal ones are taken from them by Section when X and Y are principal, otherwise they are computed
        // respect to the rotated principal axes; before, the moduli respect to the axis 1 were computed respect to X also for the section without
        // symmetry, with the principal axes rotated)

        /// <summary>
        /// Calculate the elastic modulus respect to X of the top fibre
        /// </summary>
        /// <returns>The elastic modulus respect to X of the top fibre</returns>
        protected override double CalculateWelXMax()
        {
            return Jxx / DistanceYCentroidFromTop();
        }

        /// <summary>
        /// Calculate the elastic modulus respect to X of the bottom fibre
        /// </summary>
        /// <returns>The elastic modulus respect to X of the bottom fibre</returns>
        protected override double CalculateWelXMin()
        {
            return Jxx / DistanceYCentroidFromBottom();
        }

        /// <summary>
        /// Calculate the elastic modulus respect to Y of the right fibre
        /// </summary>
        /// <returns>The elastic modulus respect to Y of the right fibre</returns>
        protected override double CalculateWelYMax()
        {
            return Jyy / DistanceXCentroidFromRight();
        }

        /// <summary>
        /// Calculate the elastic modulus respect to Y of the left fibre
        /// </summary>
        /// <returns>The elastic modulus respect to Y of the left fibre (the web)</returns>
        protected override double CalculateWelYMin()
        {
            return Jyy / DistanceXCentroidFromLeft();
        }

        /// <summary>
        /// The section is symmetric respect to X if the flanges are equal
        /// </summary>
        /// <returns>True if symmetric</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            if (_lengthTop == _lengthBottom && _tTop == _tBottom)
                return true;
            return false;
        }

        /// <summary>
        /// The section is never symmetric respect to Y
        /// </summary>
        /// <returns>False</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return false;
        }

        /// <summary>
        /// The distance of the centroid from the bottom side
        /// </summary>
        /// <returns>The distance</returns>
        public virtual double DistanceYCentroidFromBottom()
        {
            return CalculateCentroid().Y;
        }

        /// <summary>
        /// The distance of the centroid from the top side
        /// </summary>
        /// <returns>The distance</returns>
        public virtual double DistanceYCentroidFromTop()
        {
            return Height - CalculateCentroid().Y;
        }

        /// <summary>
        /// The distance of the centroid from the right end of the longest flange
        /// </summary>
        /// <returns>The distance</returns>
        public virtual double DistanceXCentroidFromRight()
        {
            return Math.Max(LengthTop, LengthBottom) - DistanceXCentroidFromLeft();
        }

        /// <summary>
        /// The distance of the centroid from the left side (the outer side of the web)
        /// </summary>
        /// <returns>The distance</returns>
        public virtual double DistanceXCentroidFromLeft()
        {
            return CalculateCentroid().X;
        }

        /// <summary>
        /// Builds the thin walls (web on its full height, flanges beyond the web), discards the mesh and the shape and calculates the properties
        /// </summary>
        private void CalculateSection()
        {
            ThinWall web = new ThinWall(Height, ThicknessWeb, Math.PI / 2.0,
                new Point2d(ThicknessWeb / 2.0, Height / 2.0));
            ThinWall flangeTop = new ThinWall(LengthTop - ThicknessWeb, ThicknessTop, 0,
                new Point2d(ThicknessWeb + (LengthTop - ThicknessWeb) / 2.0, Height - ThicknessTop * 0.5));
            ThinWall flangeBottom = new ThinWall(LengthBottom - ThicknessWeb, ThicknessBottom, 0,
                new Point2d(ThicknessWeb + (LengthBottom - ThicknessWeb) / 2.0, ThicknessBottom / 2.0));

            SetThinWalls(new ThinWall[] { web, flangeBottom, flangeTop });

            ResetMesh();

            _shape = null; // before SetMechanicalProperties (it was after: the properties computed on the shape used the old one)

            SetMechanicalProperties();
        }

        /// <summary>
        /// The area of the corners: the welds (chamfer, throat R1) or the fillets (R1) inside, minus the fillets (R2) outside (included in the
        /// area of the section, see <see cref="GetCorners"/>)
        /// </summary>
        /// <returns>The additional area</returns>
        protected double CalculateAdditionalArea()
        {
            double area = 0.0;

            foreach (SectionCorner corner in GetCorners())
                area += corner.Sign * corner.Area;

            return area;
        }

        /// <summary>
        /// The corners of the section, included by <see cref="ThinWallSection"/> in the area, in the centroid and in the moments of inertia
        /// with their exact centroids and own moments: the inside corners, welds (right triangles with legs 1.41 R1) or fillets R1, and the
        /// outside fillets R2 at the tips of the flanges, on their inner faces (also for the welded sections); nothing for the sharp corners
        /// </summary>
        /// <remarks>
        /// Before: the transport term of the fillets R1 in Jyy was R1² - R1² d² (dimensionally wrong), the outside fillets R2 were subtracted
        /// from the area and from the centroid but not from the moments of inertia and were placed outside the flanges, the centroids were
        /// R / 3.5 from the sides and the own moment of a fillet was (1 / 3 - π / 16) R⁴ (the one about the side far from it); with the
        /// transport of the thin walls (see <see cref="ThinWallSection"/>) Jyy of a UPN 300 was 4% greater and Jxy was not 0. Now area,
        /// centroid and moments of inertia are the exact ones
        /// </remarks>
        /// <returns>The corners</returns>
        private protected override SectionCorner[] GetCorners()
        {
            if (_edgeWorking == EdgeType.Sharp)
                return new SectionCorner[0];

            SectionCorner.Profile inside = SectionCorner.Inside(_edgeWorking, R1);
            SectionCorner.Profile outside = SectionCorner.Fillet(R2);

            return new SectionCorner[]
            {
                new SectionCorner(1, inside, ThicknessWeb, ThicknessBottom, 1, 1),
                new SectionCorner(1, inside, ThicknessWeb, Height - ThicknessTop, 1, -1),
                new SectionCorner(-1, outside, LengthBottom, ThicknessBottom, -1, -1),
                new SectionCorner(-1, outside, LengthTop, Height - ThicknessTop, -1, 1),
            };
        }

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Serializes the section (the version is written as "SectionCVersionVersion": the constructor reads "SectionCVersion")
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionCVersionVersion", version);

            info.AddValue("Height", _h);
            info.AddValue("ThicknessWeb", _tw);
            info.AddValue("LengthBottom", _lengthBottom);
            info.AddValue("ThicknessBottom", _tBottom);
            info.AddValue("LengthTop", _lengthTop);
            info.AddValue("ThicknessTop", _tTop);
            info.AddValue("R1", _r1);
            info.AddValue("R2", _r2);
        }

        /// <summary>
        /// The description of the section: "C h x tw x lb x tb x lt x tt"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString()
        {
            return $"C {_h}x{_tw}x{_lengthBottom}x{_tBottom}x{_lengthTop}x{_tTop} ";
        }

        /// <summary>
        /// Equality with another C section (see <see cref="Equals(SectionC)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SectionC);
        }

        /// <summary>
        /// The hash code of the section and of the dimensions
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _h.GetHashCode();
                hashCode = hashCode * -23 + _tw.GetHashCode();
                hashCode = hashCode * -23 + _lengthBottom.GetHashCode();
                hashCode = hashCode * -23 + _tBottom.GetHashCode();
                hashCode = hashCode * -23 + _lengthTop.GetHashCode();
                hashCode = hashCode * -23 + _tTop.GetHashCode();
                hashCode = hashCode * -23 + _r1.GetHashCode();
                hashCode = hashCode * -23 + _r2.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality of the section properties and of the dimensions
        /// </summary>
        /// <param name="other">The section to compare</param>
        /// <returns>True if the sections are equal</returns>
        public bool Equals(SectionC other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   _h == other._h &&
                   _tw == other._tw &&
                   _lengthBottom == other._lengthBottom &&
                   _tBottom == other._tBottom &&
                   _lengthTop == other._lengthTop &&
                   _tTop == other._tTop &&
                   _r1 == other._r1 &&
                   _r2 == other._r2;
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(SectionC)"/>; a null <paramref name="left"/> throws <see cref="NullReferenceException"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(SectionC left, SectionC right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(SectionC)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(SectionC left, SectionC right)
        {
            return !(left == right);
        }

        #endregion

    }
}
