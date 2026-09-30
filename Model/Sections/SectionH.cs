using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// An I/H section, symmetric respect to the vertical axis, with different flanges; the origin is the bottom left corner of the bounding box
    /// (the flanges are centred on the wider one)
    /// </summary>
    [Serializable]
    public class SectionH : ThinWallSection, ISerializable, IEquatable<SectionH>
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
        /// The thickness of the top flange
        /// </summary>
        protected double _ttop;
        /// <summary>
        /// The thickness of the bottom flange
        /// </summary>
        protected double _tbottom;
        /// <summary>
        /// The width of the top flange
        /// </summary>
        protected double _btop;
        /// <summary>
        /// The width of the bottom flange
        /// </summary>
        protected double _bbottom;
        /// <summary>
        /// The fillet radius or the throat of the welds
        /// </summary>
        private readonly double _r;

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
        /// The width of the wider flange
        /// </summary>
        public override double Width => Math.Max(_btop, _bbottom);

        /// <summary>
        /// The width of the bottom flange (the setter calculates the section again)
        /// </summary>
        public double LenghtBottomFlange
        {
            get => _bbottom;
            set
            {
                if (_bbottom != value)
                {
                    _bbottom = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The width of the top flange (the setter calculates the section again)
        /// </summary>
        public double LenghtTopFlange
        {
            get => _btop;
            set
            {
                if (_btop != value)
                {
                    _btop = value;
                    CalculateSection();

                }
            }
        }

        /// <summary>
        /// The thickness of the top flange (the setter calculates the section again)
        /// </summary>
        public double ThicknessTopFlange
        {
            get => _ttop;
            set
            {
                if (_ttop != value)
                {
                    _ttop = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The thickness of the bottom flange (the setter calculates the section again)
        /// </summary>
        public double ThicknessBottomFlange
        {
            get => _tbottom;
            set
            {
                if (_tbottom != value)
                {
                    _tbottom = value;
                    CalculateSection();
                }
            }
        }

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
        /// The height of the web between the flanges
        /// </summary>
        public double HeightWeb => Height - ThicknessBottomFlange - ThicknessTopFlange;

        /// <summary>
        /// Fillet radius.
        /// </summary>
        public double R => _r;

        /// <summary>
        /// The depth of the straight part of the web: height of the web minus 2 R
        /// </summary>
        public double D => Height - ThicknessBottomFlange - ThicknessTopFlange - 2.0 * R;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The height</param>
        /// <param name="thicknessWeb">The thickness of the web</param>
        /// <param name="topFlangeLength">The width of the top flange</param>
        /// <param name="topFlangeThickness">The thickness of the top flange</param>
        /// <param name="bottomFlangeLength">The width of the bottom flange</param>
        /// <param name="bottomFlangeThickness">The thickness of the bottom flange</param>
        /// <param name="name">The name</param>
        /// <param name="radius">The fillet radius or the throat of the welds (negative: 0)</param>
        /// <exception cref="ArgumentException">If a dimension is negative</exception>
        public SectionH(double height, double thicknessWeb, double topFlangeLength, double topFlangeThickness, double bottomFlangeLength,
            double bottomFlangeThickness, string name, double radius = 0)
            : base(name)
        {
            #region Check inputs

            _h = height < 0 ? throw new ArgumentException($"Web lenght cannot be lower than zero") : height;                               // altezza anima
            _tw = thicknessWeb < 0 ? throw new ArgumentException($"Web _thickness cannot be lower than zero") : thicknessWeb;                            // spessore anima
            _btop = topFlangeLength < 0 ? throw new ArgumentException($"Top flange lenght cannot be lower than zero") : topFlangeLength;                  // larghezza piattabanda superiore
            _bbottom = bottomFlangeLength < 0 ? throw new ArgumentException($"Bottom flange lenght cannot be lower than zero") : bottomFlangeLength;      // larghezza piattabanda inferiore
            _ttop = topFlangeThickness < 0 ? throw new ArgumentException($"Top flange _thickness cannot be lower than zero") : topFlangeThickness;               // spessore piattabanda superiore
            _tbottom = bottomFlangeThickness < 0 ? throw new ArgumentException($"Bottom flange _thickness cannot be lower than zero") : bottomFlangeThickness;   // spessore piattabanda inferiore
            _r = radius < 0.0 ? 0 : radius;        // altezza di gola o raggio di curvatura

            #endregion

            CalculateSection();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionH(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionHVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _h = info.GetDouble("Height");
            _bbottom = info.GetDouble("LenghtBottomFlange");
            _btop = info.GetDouble("LenghtTopFlange");
            _ttop = info.GetDouble("ThicknessTopFlange");
            _tbottom = info.GetDouble("ThicknessBottomFlange");
            _tw = info.GetDouble("ThicknessWeb");
            _r = info.GetDouble("R");
        }

        #endregion

        #region Protected override method

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionHVersion", version);

            info.AddValue("Height", _h);
            info.AddValue("LenghtBottomFlange", _bbottom);
            info.AddValue("LenghtTopFlange", _btop);
            info.AddValue("ThicknessTopFlange", _ttop);
            info.AddValue("ThicknessBottomFlange", _tbottom);
            info.AddValue("ThicknessWeb", _tw);
            info.AddValue("R", _r);
        }

        /// <summary>
        /// The shape of the section (without fillets)
        /// </summary>
        /// <returns>The new shape</returns>
        protected override Shape2d GetShape()
        {
            return new Shape2d(new Polygon2d(OutlinePoints()));
        }

        /// <summary>
        /// The twelve vertices of the shape; the inside corners between the web and the flanges are the vertices 2, 3, 8 and 9
        /// </summary>
        /// <returns>The vertices</returns>
        private Point2d[] OutlinePoints()
        {
            if (_bbottom > _btop)
            {
                return new Point2d[] {
                    new Point2d(0.0, 0.0),
                    new Point2d(0.0, _tbottom),
                    new Point2d(0.5 * (_bbottom - _tw), _tbottom),
                    new Point2d(0.5 * (_bbottom - _tw), _h - _ttop),
                    new Point2d(0.5 * (_bbottom - _btop), _h - _ttop),
                    new Point2d(0.5 * (_bbottom - _btop), _h),
                    new Point2d(0.5 * (_bbottom + _btop), _h),
                    new Point2d(0.5 * (_bbottom + _btop), _h - _ttop),
                    new Point2d(0.5 * (_bbottom + _tw), _h - _ttop),
                    new Point2d(0.5 * (_bbottom + _tw), _tbottom),
                    new Point2d(_bbottom, _tbottom),
                    new Point2d(_bbottom, 0.0) };
            }
            else
            {
                return new Point2d[] {
                    new Point2d(0.5 * (_btop - _bbottom), 0.0),
                    new Point2d(0.5 * (_btop - _bbottom), _tbottom),
                    new Point2d(0.5 * (_btop - _tw), _tbottom),
                    new Point2d(0.5 * (_btop - _tw), _h - _ttop),
                    new Point2d(0.0, _h - _ttop),
                    new Point2d(0.0, _h),
                    new Point2d(_btop, _h),
                    new Point2d(_btop, _h - _ttop),
                    new Point2d(0.5 * (_btop + _tw), _h - _ttop),
                    new Point2d(0.5 * (_btop + _tw), _tbottom),
                    new Point2d(0.5 * (_btop + _bbottom), _tbottom),
                    new Point2d(0.5 * (_btop + _bbottom), 0.0) };
            }
        }

        /// <summary>
        /// True if the section has fillets or welds in the inside corners (see <see cref="GetCorners"/>)
        /// </summary>
        private bool HasWorkedCorners => _edgeWorking != EdgeType.Sharp && R > 0.0;

        /// <summary>
        /// The region of the exact plastic moduli: the shape with the fillets or the welds of the four inside corners
        /// </summary>
        /// <returns>The outline</returns>
        private protected override Shape2d GetPlasticShape()
        {
            if (!HasWorkedCorners)
                return Shape;

            Point2d[] points = OutlinePoints();
            var vertices = new SectionOutline.Vertex[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                vertices[i] = i == 2 || i == 3 || i == 8 || i == 9
                    ? SectionOutline.Inside(points[i].X, points[i].Y, _edgeWorking, R)
                    : new SectionOutline.Vertex(points[i].X, points[i].Y);
            }
            return SectionOutline.Create(vertices);
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
            return Height - DistanceYCentroidFromBottom();
        }

        /// <summary>
        /// The distance of the centroid from the right side
        /// </summary>
        /// <returns>The distance of the centroid from the right end of the wider flange. Before, the distance from the left end</returns>
        public virtual double DistanceXCentroidFromRight()
        {
            return Math.Max(LenghtTopFlange, LenghtBottomFlange) - CalculateCentroid().X;
        }

        /// <summary>
        /// The distance of the centroid from the left side
        /// </summary>
        /// <returns>The distance of the centroid from the left end of the wider flange</returns>
        public virtual double DistanceXCentroidFromLeft()
        {
            return CalculateCentroid().X;
        }

        /// <summary>
        /// Calculate the product of inertia: 0 (symmetric section)
        /// </summary>
        /// <returns>0</returns>
        protected override double CalculateJxy()
        {
            return 0;
        }

        /// <summary>
        /// The four corners between the web and the flanges, welds (chamfer, right triangles with legs 1.41 R) or fillets (radius R), included
        /// by <see cref="ThinWallSection"/> in the area, in the centroid and in the moments of inertia with their exact centroids and own moments;
        /// nothing for the sharp corners
        /// </summary>
        /// <remarks>
        /// Before: the corners were not in the centroid (with different flanges they move it), their centroids were R / 6 from the flanges and
        /// on the axis of the web for Jyy (instead of 0.2234 R, or a / 3 for the welds, from the flange and from the face of the web), and the
        /// own moment of a fillet was (1 / 3 - π / 16) R⁴ (the one about the side far from it; exact 0.0075 R⁴) and of a weld a⁴ / 24 (exact
        /// a⁴ / 36): Jyy of a HEB 300 was 0.25% greater
        /// </remarks>
        /// <returns>The corners</returns>
        private protected override SectionCorner[] GetCorners()
        {
            if (_edgeWorking == EdgeType.Sharp)
                return new SectionCorner[0];

            SectionCorner.Profile profile = SectionCorner.Inside(_edgeWorking, R);
            double xWeb = Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2.0;
            double xLeft = xWeb - ThicknessWeb / 2.0;
            double xRight = xWeb + ThicknessWeb / 2.0;
            double yBottom = ThicknessBottomFlange;
            double yTop = Height - ThicknessTopFlange;

            return new SectionCorner[]
            {
                new SectionCorner(1, profile, xLeft, yBottom, -1, 1),
                new SectionCorner(1, profile, xRight, yBottom, 1, 1),
                new SectionCorner(1, profile, xLeft, yTop, -1, -1),
                new SectionCorner(1, profile, xRight, yTop, 1, -1),
            };
        }

        /// <summary>
        /// Calculate the plastic modulus respect to Y: twice the static moment of a half section (the two half flanges with the half web); with
        /// fillets or welds <see cref="double.NaN"/>, the exact modulus of the outline (see <see cref="GetPlasticShape"/>) is computed at the
        /// first access
        /// </summary>
        /// <remarks>Before, with fillets the half area with the fillets was multiplied by the arm of the halves without them: Wpl,z of a
        /// HE 100 AA was 6.4% greater than the published one</remarks>
        /// <returns>The plastic modulus respect to Y (the symmetry axis)</returns>
        protected override double CalculateWplY()
        {
            if (HasWorkedCorners)
                return double.NaN;

            SectionT halfSectionTop = new SectionT(LenghtTopFlange / 2.0, Height / 2.0, ThicknessTopFlange,
                ThicknessWeb / 2.0, string.Empty);
            SectionT halfSectionBottom = new SectionT(LenghtBottomFlange / 2.0, Height / 2.0, ThicknessBottomFlange,
                ThicknessWeb / 2.0, string.Empty);

            double d = (halfSectionTop.Area * (LenghtTopFlange / 2.0 - halfSectionTop.DistanceYCentroidFromBottom()) +
                halfSectionBottom.Area * (LenghtBottomFlange / 2.0 - halfSectionBottom.DistanceYCentroidFromBottom())) /
                (halfSectionBottom.Area + halfSectionTop.Area);

            return 2.0 * d * _area / 2.0;
        }

        /// <summary>
        /// Calculate the plastic modulus respect to X: closed form with the plastic neutral axis in the web or in one flange, otherwise the one of the
        /// base class; with fillets or welds <see cref="double.NaN"/>, the exact modulus of the outline (see <see cref="GetPlasticShape"/>) is
        /// computed at the first access
        /// </summary>
        /// <remarks>Before, with fillets the closed form used the area with the fillets on the geometry without them (Wpl,y up to 1.3% different
        /// from the published values)</remarks>
        /// <returns>The plastic modulus respect to X</returns>
        protected override double CalculateWplX()
        {
            if (HasWorkedCorners)
                return double.NaN;

            if (_area / 2.0 >= LenghtTopFlange * ThicknessTopFlange && _area / 2.0 >= LenghtBottomFlange * ThicknessBottomFlange)
            {
                double hw = (_area / 2.0 - LenghtTopFlange * ThicknessTopFlange) / ThicknessWeb;

                SectionT halfSectionTop = new SectionT(hw + ThicknessTopFlange, LenghtTopFlange, ThicknessWeb,
                    ThicknessTopFlange, string.Empty);
                SectionT halfSectionBottom = new SectionT(Height - ThicknessTopFlange - hw, LenghtBottomFlange,
                    ThicknessWeb, ThicknessBottomFlange, string.Empty);

                return _area / 2.0 * (halfSectionTop.DistanceYCentroidFromBottom() + halfSectionBottom.DistanceYCentroidFromBottom());
            }
            else if (_area / 2.0 <= LenghtTopFlange * ThicknessTopFlange)
            {
                double hHalf = _area / 2.0 / LenghtTopFlange;

                SectionH halfSectionBottom = new SectionH(Height - hHalf, ThicknessWeb, LenghtTopFlange,
                    ThicknessTopFlange - hHalf, LenghtBottomFlange, ThicknessBottomFlange, string.Empty);

                return _area / 2.0 * (hHalf / 2.0 + (Height - hHalf - halfSectionBottom.DistanceYCentroidFromBottom()));
            }
            else if (_area / 2.0 <= LenghtBottomFlange * ThicknessBottomFlange)
            {
                double hHalf = _area / 2.0 / LenghtBottomFlange;

                SectionH halfSectionBottom = new SectionH(Height - hHalf, ThicknessWeb, LenghtTopFlange, ThicknessTopFlange,
                    LenghtBottomFlange, ThicknessBottomFlange - hHalf, string.Empty);

                return _area / 2.0 * (hHalf / 2.0 + halfSectionBottom.DistanceYCentroidFromBottom());
            }
            else
                return base.CalculateWplX(); // plastic neutral axis in both flanges: the modulus of ThinWallSection (before, an exception in the constructor)
        }

        // The moduli respect to X and Y (the principal ones are taken from them by Section: the axis 1 is X for the usual sections, Y when Jyy is
        // bigger than Jxx). Respect to Y the extreme fibres are the ends of the wider flange, on both sides (before, the length of each flange minus
        // the distance of the centroid from the left end: with different flanges one modulus was wrong, infinite or negative, e.g. flanges
        // 100 and 300: Wel2Max = J22 / (100 - 150) < 0, so Wel2 < 0)

        /// <summary>
        /// Calculate the elastic modulus respect to X of the bottom fibre
        /// </summary>
        /// <returns>The elastic modulus respect to X of the bottom fibre</returns>
        protected override double CalculateWelXMin()
        {
            return Jxx / DistanceYCentroidFromBottom();
        }

        /// <summary>
        /// Calculate the elastic modulus respect to X of the top fibre
        /// </summary>
        /// <returns>The elastic modulus respect to X of the top fibre</returns>
        protected override double CalculateWelXMax()
        {
            return Jxx / DistanceYCentroidFromTop();
        }

        /// <summary>
        /// Calculate the elastic modulus respect to Y of the left fibre
        /// </summary>
        /// <returns>The elastic modulus respect to Y of the left fibre</returns>
        protected override double CalculateWelYMin()
        {
            return Jyy / DistanceXCentroidFromLeft();
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
        /// The section is symmetric respect to X if the flanges are equal
        /// </summary>
        /// <returns>True if symmetric</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            if (_btop == _bbottom && _tbottom == _ttop)
                return true;

            return false;
        }

        /// <summary>
        /// The section is always symmetric respect to Y
        /// </summary>
        /// <returns>True</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return true;
        }

        /// <summary>
        /// Calculate the shear center (CNR DT 208/2011, to be checked): on the vertical axis, moved towards the flange with the bigger moment of
        /// inertia
        /// </summary>
        /// <returns>The shear center</returns>
        protected override Point2d CalculateShearCenter()
        {
            //CNR DT208_2011 --> to be checked
            double JFlTop = 1.0 / 12.0 * ThicknessTopFlange * Math.Pow(LenghtTopFlange, 3.0);
            double JFlBottom = 1.0 / 12.0 * ThicknessBottomFlange * Math.Pow(LenghtBottomFlange, 3.0);
            double jz = JFlTop + JFlBottom + 1.0 / 12.0 * HeightWeb * Math.Pow(ThicknessWeb, 3.0);
            double zBottom = CalculateCentroid().Y - ThicknessBottomFlange / 2.0;
            double zTop = Height - ThicknessTopFlange / 2.0 - CalculateCentroid().Y;

            return new Point2d(CalculateCentroid().X, CalculateCentroid().Y - (zBottom * JFlBottom - zTop * JFlTop) / jz);
        }

        /// <summary>
        /// Calculate the torsion constant: with fillets the formula of the rolled I sections (mean flange, α1 and D1 of the fillets), otherwise
        /// the one of the thin walls
        /// </summary>
        /// <returns>The torsion constant</returns>
        protected override double CalculateJt()
        {
            if (_edgeWorking == EdgeType.Fillet)
            {
                double b = (LenghtBottomFlange + LenghtTopFlange) / 2;
                double tf = (ThicknessBottomFlange + ThicknessTopFlange) / 2;

                double alpha1 = -0.042 + 0.2204 * ThicknessWeb / tf + 0.1355 * R / tf -
                    0.0865 * R * ThicknessWeb / Math.Pow(tf, 2) - 0.0725 * Math.Pow(ThicknessWeb, 2) / Math.Pow(tf, 2);
                double D1 = (Math.Pow(tf + R, 2.0) + (R + 0.25 * ThicknessWeb) * ThicknessWeb) / (2.0 * R + tf);

                return (2.0 / 3.0) * b * Math.Pow(tf, 3) + (1.0 / 3.0) * (Height - 2 * tf) * Math.Pow(ThicknessWeb, 3) +
                    2.0 * alpha1 * Math.Pow(D1, 4) - 0.420 * Math.Pow(tf, 4);
            }
            else
                return base.CalculateJt();
        }

        /// <summary>
        /// Calculate the warping constant (CNR DT 208/2011): d² If,bottom If,top / Iz, with d the distance of the flange middle lines
        /// </summary>
        /// <returns>The warping constant</returns>
        protected override double CalculateJw()
        {
            double dmed = _h - ThicknessBottomFlange / 2.0 - ThicknessTopFlange / 2.0;
            double JFlTop = 1.0 / 12.0 * ThicknessTopFlange * Math.Pow(LenghtTopFlange, 3.0);
            double JFlBottom = 1.0 / 12.0 * ThicknessBottomFlange * Math.Pow(LenghtBottomFlange, 3.0);
            double jz = JFlTop + JFlBottom + 1.0 / 12.0 * HeightWeb * Math.Pow(ThicknessWeb, 3.0);

            // CNR DT208_2011
            return dmed * dmed * JFlBottom * JFlTop / jz;
        }

        /// <summary>
        /// Builds the thin walls (web between the flanges, flanges on their full width), discards the mesh and the shape and calculates the properties
        /// </summary>
        private void CalculateSection()
        {
            ThinWall web = new ThinWall(HeightWeb, ThicknessWeb, Math.PI / 2,
                new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2.0, ThicknessBottomFlange + HeightWeb / 2.0));
            ThinWall flangeTop = new ThinWall(LenghtTopFlange, ThicknessTopFlange, 0,
                new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2.0, ThicknessBottomFlange + HeightWeb + ThicknessTopFlange / 2.0));
            ThinWall flangeBottom = new ThinWall(LenghtBottomFlange, ThicknessBottomFlange, 0,
                new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2.0, ThicknessBottomFlange / 2.0));

            SetThinWalls(new ThinWall[3] { web, flangeTop, flangeBottom });

            ResetMesh();

            _shape = null; // before SetMechanicalProperties (it was after: the properties computed on the shape used the old one)

            SetMechanicalProperties();
        }

        /// <summary>
        /// The description of the section: "H h x tw x bb x tb x bt x tt"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString()
        {
            return $"H {_h}x{_tw}x{_bbottom}x{_tbottom}x{_btop}x{_ttop}";
        }

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Equality with another H section (see <see cref="Equals(SectionH)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SectionH);
        }

        /// <summary>
        /// Equality of the section properties and of the dimensions
        /// </summary>
        /// <param name="other">The section to compare</param>
        /// <returns>True if the sections are equal</returns>
        public bool Equals(SectionH other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   _h == other._h &&
                   _tw == other._tw &&
                   _ttop == other._ttop &&
                   _tbottom == other._tbottom &&
                   _btop == other._btop &&
                   _bbottom == other._bbottom &&
                   _r == other._r;
        }

        /// <summary>
        /// The hash code of the section and of the dimensions
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -1753634061;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + _h.GetHashCode();
                hashCode = hashCode * -1521134295 + _tw.GetHashCode();
                hashCode = hashCode * -1521134295 + _ttop.GetHashCode();
                hashCode = hashCode * -1521134295 + _tbottom.GetHashCode();
                hashCode = hashCode * -1521134295 + _btop.GetHashCode();
                hashCode = hashCode * -1521134295 + _bbottom.GetHashCode();
                hashCode = hashCode * -1521134295 + _r.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(SectionH)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(SectionH left, SectionH right)
        {
            return EqualityComparer<SectionH>.Default.Equals(left, right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(SectionH)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(SectionH left, SectionH right)
        {
            return !(left == right);
        }

        #endregion
    }
}
