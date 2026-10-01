using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// An angle (L) section: a horizontal leg on the bottom and a vertical leg on the left; the origin is the outer corner
    /// </summary>
    [Serializable]
    public class SectionL : ThinWallSection, ISerializable, IEquatable<SectionL>
    {
        #region Variables

        /// <summary>
        /// The length of the horizontal leg
        /// </summary>
        private double _horizontalLegLength;
        /// <summary>
        /// The thickness of the horizontal leg
        /// </summary>
        private double _horizontalLegThickness;
        /// <summary>
        /// The length of the vertical leg
        /// </summary>
        private double _verticalLegLength;
        /// <summary>
        /// The thickness of the vertical leg
        /// </summary>
        private double _verticalLegThickness;

        /// <summary>
        /// The root fillet radius (rolled) or the throat of the weld (welded) of the inside corner
        /// </summary>
        private readonly double _r;
        /// <summary>
        /// The toe radius of the ends of the legs, on the inner side (rolled)
        /// </summary>
        private readonly double _r2;

        #endregion

        #region Properties

        /// <summary>
        /// The length of the horizontal leg (the setter calculates the section again)
        /// </summary>
        public double HorizontalLegLength
        {
            get => _horizontalLegLength;
            set
            {
                if (_horizontalLegLength != value)
                {
                    _horizontalLegLength = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The thickness of the horizontal leg (the setter calculates the section again)
        /// </summary>
        public double HorizontalLegThickness
        {
            get => _horizontalLegThickness;
            set
            {
                if (_horizontalLegThickness != value)
                {
                    _horizontalLegThickness = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The length of the vertical leg (the setter calculates the section again)
        /// </summary>
        public double VerticalLegLength
        {
            get => _verticalLegLength;
            set
            {
                if (_verticalLegLength != value)
                {
                    _verticalLegLength = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The thickness of the vertical leg (the setter calculates the section again)
        /// </summary>
        public double VerticalLegThickness
        {
            get => _verticalLegThickness;
            set
            {
                if (_verticalLegThickness != value)
                {
                    _verticalLegThickness = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The root fillet radius (rolled) or the throat of the weld (welded) of the inside corner: in the calculations when the working of the
        /// corners is set (see <see cref="ThinWallSection.SetEdgeTypeFromSteelType"/>)
        /// </summary>
        public double R => _r;

        /// <summary>
        /// The toe radius of the ends of the legs, on the inner side: in the calculations of the rolled sections (fillet working)
        /// </summary>
        public double R2 => _r2;

        /// <summary>
        /// The height: the length of the vertical leg
        /// </summary>
        public override double Height
        {
            get => VerticalLegLength;
            set => VerticalLegLength = value;
        }

        /// <summary>
        /// The width: the length of the horizontal leg
        /// </summary>
        public override double Width => _horizontalLegLength;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor: creates the section and calculates its properties.
        /// Thin walls with _angle = 0:
        /// <code>
        ///  ▲ Y
        ///  │
        ///  │
        ///  ┌───┐
        ///  │   │
        ///  │   │
        ///  │   │
        ///  │   │
        ///  │   │
        ///  ├───┴─────────────────┐
        ///  │                     │
        ///  └─────────────────────┘ ────► X
        /// </code>
        /// </summary>
        /// <param name="horizontalLegLength">The horizontal leg length</param>
        /// <param name="horizontalLegThickness">The horizontal leg thickness</param>
        /// <param name="verticalLegLength">The vertical leg length</param>
        /// <param name="verticalLegThickness">The vertical leg thickness</param>
        /// <param name="name">Name of the section</param>
        /// <param name="radius">The root fillet radius or the throat of the weld (negative: 0), in the calculations when the working of the
        /// corners is set (before, never used)</param>
        /// <exception cref="ArgumentException">If a dimension is negative</exception>
        public SectionL(double horizontalLegLength, double horizontalLegThickness, double verticalLegLength, double verticalLegThickness,
            string name, double radius = 0)
            : this(horizontalLegLength, horizontalLegThickness, verticalLegLength, verticalLegThickness, name, radius, 0)
        {
        }

        /// <summary>
        /// Creates the section with the root fillet and the toe radii of a rolled angle and calculates its properties (the radii are in the
        /// calculations when the working of the corners is set, see <see cref="ThinWallSection.SetEdgeTypeFromSteelType"/>)
        /// </summary>
        /// <param name="horizontalLegLength">The horizontal leg length</param>
        /// <param name="horizontalLegThickness">The horizontal leg thickness</param>
        /// <param name="verticalLegLength">The vertical leg length</param>
        /// <param name="verticalLegThickness">The vertical leg thickness</param>
        /// <param name="name">Name of the section</param>
        /// <param name="radius">The root fillet radius or the throat of the weld (negative: 0)</param>
        /// <param name="toeRadius">The toe radius of the ends of the legs, on the inner side (negative: 0)</param>
        /// <exception cref="ArgumentException">If a dimension is negative</exception>
        public SectionL(double horizontalLegLength, double horizontalLegThickness, double verticalLegLength, double verticalLegThickness,
            string name, double radius, double toeRadius)
            : base(name)
        {
            _horizontalLegLength = horizontalLegLength < 0 ? throw new ArgumentException($"Horizzontal plate lenght cannot be lower than zero") : horizontalLegLength;
            _horizontalLegThickness = horizontalLegThickness < 0 ? throw new ArgumentException($"Horizzontal plate _thickness cannot be lower than zero") : horizontalLegThickness;
            _verticalLegLength = verticalLegLength < 0 ? throw new ArgumentException($"Vertical plate lenght cannot be lower than zero") : verticalLegLength;
            _verticalLegThickness = verticalLegThickness < 0 ? throw new ArgumentException($"Vertical plate _thickness cannot be lower than zero") : verticalLegThickness;
            _r = radius < 0 ? 0 : radius;        // raggio di curvatura o altezza di gola
            _r2 = toeRadius < 0 ? 0 : toeRadius;

            ThinWall thinWall1 = new ThinWall(HorizontalLegLength, HorizontalLegThickness, 0,
                new Point2d(HorizontalLegLength / 2, HorizontalLegThickness / 2));
            ThinWall thinWall2 = new ThinWall(VerticalLegLength - HorizontalLegThickness, VerticalLegThickness, Math.PI / 2,
                new Point2d(VerticalLegThickness / 2, HorizontalLegThickness + (VerticalLegLength - HorizontalLegThickness) / 2));

            SetThinWalls(new ThinWall[] { thinWall1, thinWall2 });

            SetMechanicalProperties();
            ResetMesh();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionL(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionLVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _horizontalLegLength = info.GetDouble("HorizontalLegLength");
            _horizontalLegThickness = info.GetDouble("HorizontalLegThickness");
            _verticalLegLength = info.GetDouble("VerticalLegLength");
            _verticalLegThickness = info.GetDouble("VerticalLegThickness");
            _r = info.GetDouble("R");
            if (version >= 3)
                _r2 = info.GetDouble("R2");
        }

        #endregion

        #region Protected method

        /// <summary>
        /// Calculates the properties: area, centroid, moments of inertia, principal axes and the elastic moduli from the five vertices of the
        /// shape (respect to X and Y and to the principal axes)
        /// </summary>
        public override void SetMechanicalProperties()
        {
            _area = CalculateArea();

            _centroid = CalculateCentroid();
            _isSymmetricAlongXLocalAxis = CalculateIsSymmetricAlongXLocalAxis();
            _isSymmetricAlongYLocalAxis = CalculateIsSymmetricAlongYLocalAxis();

            _jxx = CalculateJxx();
            _jyy = CalculateJyy();
            _jxy = CalculateJxy();

            // J11 and J22 before the angle, that compares them with Jxx and Jyy (before, the values of the previous calculation were used)
            _j11 = CalculateJ11();
            _j22 = CalculateJ22();
            _angleX1 = CalculateAngle();

            _jp = _jxx + _jyy;
            _jt = CalculateJt();
            _jw = CalculateJw();

            _shearCenter = CalculateShearCenter();

            // the moduli respect to X and Y first: the plastic moduli respect to the principal axes are taken from them when X and Y are principal
            var welL = CalculateWel(0.0, _jxx, _jyy);
            _welXMax = welL.WelTop;
            _welXMin = welL.WelBottom;
            _welYMax = welL.WelRight;
            _welYMin = welL.WelLeft;
            _wplX = CalculateWplX();
            _wplY = CalculateWplY();

            // respect to the principal axes: top/bottom = maximum/minimum coordinate y1, right/left = maximum/minimum coordinate x1
            var (WelTop, WelBottom, WelLeft, WelRight) = CalculateWel(_angleX1, _j11, _j22);
            _wel1Max = WelTop;
            _wel1Min = WelBottom;
            _wel2Max = WelRight;
            _wel2Min = WelLeft;
            _wpl1 = CalculateWpl1();
            _wpl2 = CalculateWpl2();
        }

        /// <summary>
        /// Calculate the elastic modulus respect to X of the top fibre
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWelXMax()
        {
            return CalculateWel(0.0, _jxx, _jyy).WelTop;
        }

        /// <summary>
        /// Calculate the elastic modulus respect to X of the bottom fibre
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWelXMin()
        {
            return CalculateWel(0.0, _jxx, _jyy).WelBottom;
        }

        /// <summary>
        /// Calculate the elastic modulus respect to Y of the right fibre
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWelYMax()
        {
            return CalculateWel(0.0, _jxx, _jyy).WelRight;
        }

        /// <summary>
        /// Calculate the elastic modulus respect to Y of the left fibre
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWelYMin()
        {
            return CalculateWel(0.0, _jxx, _jyy).WelLeft;
        }

        /// <summary>
        /// Calculate the elastic modulus respect to the axis 1 of the fibre with the maximum coordinate y1
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWel1Max()
        {
            return CalculateWel(_angleX1, _j11, _j22).WelTop;
        }

        /// <summary>
        /// Calculate the elastic modulus respect to the axis 1 of the fibre with the minimum coordinate y1
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWel1Min()
        {
            return CalculateWel(_angleX1, _j11, _j22).WelBottom;
        }

        /// <summary>
        /// Calculate the elastic modulus respect to the axis 2 of the fibre with the maximum coordinate x1
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWel2Max()
        {
            return CalculateWel(_angleX1, _j11, _j22).WelRight;
        }

        /// <summary>
        /// Calculate the elastic modulus respect to the axis 2 of the fibre with the minimum coordinate x1
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWel2Min()
        {
            return CalculateWel(_angleX1, _j11, _j22).WelLeft;
        }

        /// <summary>
        /// The elastic moduli respect to the axes through the centroid rotated by an angle, from the extreme vertices
        /// </summary>
        /// <param name="teta">The angle of the axes from X (radians)</param>
        /// <param name="Jxx">The moment of inertia about the rotated x axis</param>
        /// <param name="Jyy">The moment of inertia about the rotated y axis</param>
        /// <returns>The moduli of the top, bottom, left and right fibres</returns>
        private (double WelTop, double WelBottom, double WelLeft, double WelRight) CalculateWel(in double teta, in double Jxx, in double Jyy)
        {
            var (minX, maxX, minY, maxY) = FivePointsCheck(teta);
            double WelTop = Jxx / Math.Abs(maxY);
            double WelBottom = Jxx / Math.Abs(minY);
            double WelLeft = Jyy / Math.Abs(minX);
            double WelRight = Jyy / Math.Abs(maxX);

            return (WelTop, WelBottom, WelLeft, WelRight);
        }

        /// <summary>
        /// The extreme coordinates of the five outer vertices of the shape in the axes through the centroid rotated by an angle
        /// </summary>
        /// <param name="angle">The angle of the axes from X (radians)</param>
        /// <returns>The minimum and maximum x and y</returns>
        private (double minX, double maxX, double minY, double maxY) FivePointsCheck(in double angle)
        {
            //check 5 points
            //traslation
            Point2d[] pts = new Point2d[5];
            pts[0] = new Point2d(-Centroid.X, -Centroid.Y);
            pts[1] = new Point2d(HorizontalLegLength - Centroid.X, -Centroid.Y);
            pts[2] = new Point2d(HorizontalLegLength - Centroid.X, HorizontalLegThickness - Centroid.Y);
            pts[3] = new Point2d(VerticalLegThickness - Centroid.X, VerticalLegLength - Centroid.Y);
            pts[4] = new Point2d(-Centroid.X, VerticalLegLength - Centroid.Y);

            //rotation
            double minX = 0;
            double maxX = 0;
            double minY = 0;
            double maxY = 0;
            for (int i = 0; i < 5; i++)
            {
                double x = pts[i].X;
                double y = pts[i].Y;
                double newX = x * Math.Cos(angle) + y * Math.Sin(angle);
                double newY = -x * Math.Sin(angle) + y * Math.Cos(angle);
                pts[i] = new Point2d(newX, newY);

                minX = Math.Min(minX, pts[i].X);
                maxX = Math.Max(maxX, pts[i].X);
                minY = Math.Min(minY, pts[i].Y);
                maxY = Math.Max(maxY, pts[i].Y);
            }

            return (minX, maxX, minY, maxY);
        }

        /// <summary>
        /// The corners of the rolled or welded angle, included by <see cref="ThinWallSection"/> in the area, in the centroid and in the moments
        /// of inertia: the root fillet (radius R) or weld (throat R) of the inside corner and, for the rolled angle, the toe radii R2 of the ends
        /// of the legs, on their inner side; nothing for the sharp corners
        /// </summary>
        /// <returns>The corners</returns>
        private protected override SectionCorner[] GetCorners()
        {
            if (_edgeWorking == EdgeType.Sharp)
                return new SectionCorner[0];

            var corners = new List<SectionCorner>
            {
                new SectionCorner(1, SectionCorner.Inside(_edgeWorking, _r), _verticalLegThickness, _horizontalLegThickness, 1, 1),
            };
            if (_edgeWorking == EdgeType.Fillet && _r2 > 0.0)
            {
                SectionCorner.Profile toe = SectionCorner.Fillet(_r2);
                corners.Add(new SectionCorner(-1, toe, _horizontalLegLength, _horizontalLegThickness, -1, -1));
                corners.Add(new SectionCorner(-1, toe, _verticalLegThickness, _verticalLegLength, -1, -1));
            }
            return corners.ToArray();
        }

        /// <summary>
        /// The region of the exact plastic moduli: the shape with the corners of <see cref="GetCorners"/>
        /// </summary>
        /// <returns>The outline</returns>
        internal override Shape2d GetPlasticShape()
        {
            if (_edgeWorking == EdgeType.Sharp)
                return Shape;

            double toe = _edgeWorking == EdgeType.Fillet ? _r2 : 0.0;
            return SectionOutline.Create(new[]
            {
                new SectionOutline.Vertex(0.0, 0.0),
                new SectionOutline.Vertex(0.0, _verticalLegLength),
                new SectionOutline.Vertex(_verticalLegThickness, _verticalLegLength, toe),
                SectionOutline.Inside(_verticalLegThickness, _horizontalLegThickness, _edgeWorking, _r),
                new SectionOutline.Vertex(_horizontalLegLength, _horizontalLegThickness, toe),
                new SectionOutline.Vertex(_horizontalLegLength, 0.0),
            });
        }

        /// <summary>
        /// Calculate the moment of inertia about X through the centroid: the one about the top side minus the transport term; with the
        /// corners (rolled or welded angle) the thin walls plus the corners of <see cref="ThinWallSection"/>
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJxx()
        {
            if (_edgeWorking != EdgeType.Sharp)
                return base.CalculateJxx();

            return (1.0 / 3.0) * (HorizontalLegLength * Math.Pow(VerticalLegLength, 3) - (HorizontalLegLength - VerticalLegThickness) * Math.Pow(VerticalLegLength - HorizontalLegThickness, 3)) -
                Area * Math.Pow(VerticalLegLength - Centroid.Y, 2);
        }

        /// <summary>
        /// Calculate the moment of inertia about Y through the centroid: the one about the right side minus the transport term; with the
        /// corners the thin walls plus the corners of <see cref="ThinWallSection"/>
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJyy()
        {
            if (_edgeWorking != EdgeType.Sharp)
                return base.CalculateJyy();

            return (1.0 / 3.0) * (VerticalLegLength * Math.Pow(HorizontalLegLength, 3) - (VerticalLegLength - HorizontalLegThickness) * Math.Pow(HorizontalLegLength - VerticalLegThickness, 3)) -
                Area * Math.Pow(HorizontalLegLength - Centroid.X, 2);
        }

        /// <summary>
        /// The shape of the section (without radius)
        /// </summary>
        /// <returns>The new shape</returns>
        protected override Shape2d GetShape()
        {
            return new Shape2d(new Polygon2d(new Point2d[] {
                new Point2d(0.0, 0.0),
                new Point2d(0.0, _verticalLegLength),
                new Point2d(_verticalLegThickness, _verticalLegLength),
                new Point2d(_verticalLegThickness, _horizontalLegThickness),
                new Point2d(_horizontalLegLength, _horizontalLegThickness),
                new Point2d(_horizontalLegLength, 0.0) }));
        }

        /// <summary>
        /// Calculate the warping constant (CNR DT 208/2011)
        /// </summary>
        /// <returns>The warping constant</returns>
        protected override double CalculateJw()
        {
            return (Math.Pow(_horizontalLegLength - _verticalLegThickness / 2.0, 3.0) * Math.Pow(_horizontalLegThickness, 3.0) + Math.Pow(_verticalLegLength - _horizontalLegThickness / 2.0, 3.0) * Math.Pow(_verticalLegThickness, 3.0)) / 36.0; //CNR DT 208/2011
        }

        /// <summary>
        /// Calculate the torsion constant: sum of b t³ / 3 of the legs (middle line lengths)
        /// </summary>
        /// <returns>The torsion constant</returns>
        protected override double CalculateJt()
        {
            return 1.0 / 3.0 * (_horizontalLegLength - _verticalLegThickness / 2.0) * Math.Pow(_horizontalLegThickness, 3.0) + 1.0 / 3.0 * (_verticalLegLength - _horizontalLegThickness / 2.0) * Math.Pow(_verticalLegThickness, 3.0);
        }

        /// <summary>
        /// Calculate the shear center: the intersection of the middle lines of the legs
        /// </summary>
        /// <returns>The shear center</returns>
        protected override Point2d CalculateShearCenter()
        {
            return new Point2d(_verticalLegThickness / 2.0, _horizontalLegThickness / 2.0);
        }

        /// <summary>
        /// Calculate the centroid from the static moments of the two thin walls (with the corners, the one of <see cref="ThinWallSection"/>)
        /// </summary>
        /// <returns>The centroid</returns>
        protected override Point2d CalculateCentroid()
        {
            if (_edgeWorking != EdgeType.Sharp)
                return base.CalculateCentroid();

            double xc = (_thinWalls[0].CalculateSy() + _thinWalls[1].CalculateSy()) / Area;
            double yc = (_thinWalls[0].CalculateSx() + _thinWalls[1].CalculateSx()) / Area;
            return new Point2d(xc, yc);
        }

        /// <summary>
        /// Builds the thin walls, discards the mesh and the shape and calculates the properties
        /// </summary>
        private void CalculateSection()
        {
            ThinWall thinWall1 = new ThinWall(HorizontalLegLength, HorizontalLegThickness, 0,
                new Point2d(HorizontalLegLength / 2, HorizontalLegThickness / 2));
            ThinWall thinWall2 = new ThinWall(VerticalLegLength - HorizontalLegThickness, VerticalLegThickness, Math.PI / 2,
                new Point2d(VerticalLegThickness / 2, HorizontalLegThickness + (VerticalLegLength - HorizontalLegThickness) / 2));

            SetThinWalls(new ThinWall[] { thinWall1, thinWall2 });

            ResetMesh();

            _shape = null; // before SetMechanicalProperties (it was after: the properties computed on the shape used the old one)

            SetMechanicalProperties();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The description of the section (the last value is the type name of the thin walls array, not the thickness of the horizontal leg)
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString()
        {
            return $"L {_verticalLegLength}x{_verticalLegThickness}x{_horizontalLegLength}x{_thinWalls}";
        }

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 3; // 3: the toe radius R2
            info.AddValue("SectionLVersion", version);

            info.AddValue("HorizontalLegLength", _horizontalLegLength);
            info.AddValue("HorizontalLegThickness", _horizontalLegThickness);
            info.AddValue("VerticalLegLength", _verticalLegLength);
            info.AddValue("VerticalLegThickness", _verticalLegThickness);
            info.AddValue("R", _r);
            info.AddValue("R2", _r2);
        }

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Equality with another L section (see <see cref="Equals(SectionL)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SectionL);
        }

        /// <summary>
        /// Equality of the section properties and of the dimensions
        /// </summary>
        /// <param name="other">The section to compare</param>
        /// <returns>True if the sections are equal</returns>
        public bool Equals(SectionL other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   _horizontalLegLength == other._horizontalLegLength &&
                   _horizontalLegThickness == other._horizontalLegThickness &&
                   _verticalLegLength == other._verticalLegLength &&
                   _verticalLegThickness == other._verticalLegThickness &&
                   _r == other._r &&
                   _r2 == other._r2;
        }

        /// <summary>
        /// The hash code of the section and of the dimensions
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -1338788454;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + _horizontalLegLength.GetHashCode();
                hashCode = hashCode * -1521134295 + _horizontalLegThickness.GetHashCode();
                hashCode = hashCode * -1521134295 + _verticalLegLength.GetHashCode();
                hashCode = hashCode * -1521134295 + _verticalLegThickness.GetHashCode();
                hashCode = hashCode * -1521134295 + _r.GetHashCode();
                hashCode = hashCode * -1521134295 + _r2.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(SectionL)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(SectionL left, SectionL right)
        {
            return EqualityComparer<SectionL>.Default.Equals(left, right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(SectionL)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(SectionL left, SectionL right)
        {
            return !(left == right);
        }

        #endregion
    }
}
