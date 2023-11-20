using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    [Serializable]
    public class SectionL : ThinWallSection, ISerializable, IEquatable<SectionL>
    {
        #region Variables

        private double _horizontalLegLength;
        private double _horizontalLegThickness;
        private double _verticalLegLength;
        private double _verticalLegThickness;

        private readonly double _r;                // raggio di curvatura o altezza di gola

        #endregion

        #region Properties

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

        public double R => _r;

        public override double Height
        {
            get => VerticalLegLength;
            set => VerticalLegLength = value;
        }

        public override double Width => _horizontalLegLength;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// 
        /// Thin walls with _angle = 0:
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
        /// </summary>
        /// <param name="horizontalLegLength">The horizontal leg length</param>
        /// <param name="horizontalLegThickness">The horizontal leg _thickness</param>
        /// <param name="verticalLegLength">The vertical leg length</param>
        /// <param name="verticalLegThickness">The vertical leg _thickness</param>
        /// <param name="name">Name of the section</param>
        public SectionL(double horizontalLegLength, double horizontalLegThickness, double verticalLegLength, double verticalLegThickness,
            string name, double radius = 0)
            : base(name)
        {
            _horizontalLegLength = horizontalLegLength < 0 ? throw new ArgumentException($"Horizzontal plate lenght cannot be lower than zero") : horizontalLegLength;
            _horizontalLegThickness = horizontalLegThickness < 0 ? throw new ArgumentException($"Horizzontal plate _thickness cannot be lower than zero") : horizontalLegThickness;
            _verticalLegLength = verticalLegLength < 0 ? throw new ArgumentException($"Vertical plate lenght cannot be lower than zero") : verticalLegLength;
            _verticalLegThickness = verticalLegThickness < 0 ? throw new ArgumentException($"Vertical plate _thickness cannot be lower than zero") : verticalLegThickness;
            _r = radius < 0 ? 0 : radius;        // raggio di curvatura o altezza di gola

            ThinWall thinWall1 = new ThinWall(HorizontalLegLength, HorizontalLegThickness, 0,
                new Point2d(HorizontalLegLength / 2, HorizontalLegThickness / 2));
            ThinWall thinWall2 = new ThinWall(VerticalLegLength - HorizontalLegThickness, VerticalLegThickness, Math.PI / 2,
                new Point2d(VerticalLegThickness / 2, HorizontalLegThickness + (VerticalLegLength - HorizontalLegThickness) / 2));

            SetThinWalls(new ThinWall[] { thinWall1, thinWall2 });

            SetMechanicalProperties();
            _mesh = GetMesh();
        }

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
        }

        #endregion

        #region Protected method

        public override void SetMechanicalProperties()
        {
            _area = CalculateArea();

            _centroid = CalculateCentroid();
            _isSymmetricAlongXLocalAxis = CalculateIsSymmetricAlongXLocalAxis();
            _isSymmetricAlongYLocalAxis = CalculateIsSymmetricAlongYLocalAxis();

            _jxx = CalculateJxx();
            _jyy = CalculateJyy();
            _jxy = CalculateJxy();

            _angleX1 = CalculateAngle();
            _j11 = CalculateJ11();
            _j22 = CalculateJ22();

            _jp = _jxx + _jyy;
            _jt = CalculateJt();
            _jw = CalculateJw();

            _shearCenter = CalculateShearCenter();

            var (WelTop, WelBottom, WelLeft, WelRight) = CalculateWel(_angleX1, _j11, _j22);
            _wel1Max = WelTop;
            _wel1Min = WelBottom;
            _wel2Max = WelRight;
            _wel2Min = WelLeft;
            _wpl1 = CalculateWpl1();
            _wpl2 = CalculateWpl2();

            var welL = CalculateWel(0.0, _jxx, _jyy);
            _welXMax = welL.WelTop;
            _welXMin = welL.WelBottom;
            _welYMax = welL.WelRight;
            _welYMin = welL.WelLeft;
            _wplX = CalculateWplX();
            _wplY = CalculateWplY();
        }

        protected override double CalculateWelXMax()
        {
            return CalculateWel(0.0, _jxx, _jyy).WelTop;
        }

        protected override double CalculateWelXMin()
        {
            return CalculateWel(0.0, _jxx, _jyy).WelBottom;
        }

        protected override double CalculateWelYMax()
        {
            return CalculateWel(0.0, _jxx, _jyy).WelRight;
        }

        protected override double CalculateWelYMin()
        {
            return CalculateWel(0.0, _jxx, _jyy).WelLeft;
        }

        protected override double CalculateWel1Max()
        {
            return CalculateWel(_angleX1, _j11, _j22).WelTop;
        }

        protected override double CalculateWel1Min()
        {
            return CalculateWel(_angleX1, _j11, _j22).WelBottom;
        }

        protected override double CalculateWel2Max()
        {
            return CalculateWel(_angleX1, _j11, _j22).WelRight;
        }

        protected override double CalculateWel2Min()
        {
            return CalculateWel(_angleX1, _j11, _j22).WelLeft;
        }

        private (double WelTop, double WelBottom, double WelLeft, double WelRight) CalculateWel(in double teta, in double Jxx, in double Jyy)
        {
            var (minX, maxX, minY, maxY) = FivePointsCheck(teta);
            double WelTop = Jxx / Math.Abs(maxY);
            double WelBottom = Jxx / Math.Abs(minY);
            double WelLeft = Jyy / Math.Abs(minX);
            double WelRight = Jyy / Math.Abs(maxX);

            return (WelTop, WelBottom, WelLeft, WelRight);
        }

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

        protected override double CalculateJxx()
        {
            return (1.0 / 3.0) * (HorizontalLegLength * Math.Pow(VerticalLegLength, 3) - (HorizontalLegLength - VerticalLegThickness) * Math.Pow(VerticalLegLength - HorizontalLegThickness, 3)) -
                Area * Math.Pow(VerticalLegLength - Centroid.Y, 2);
        }

        protected override double CalculateJyy()
        {
            return (1.0 / 3.0) * (VerticalLegLength * Math.Pow(HorizontalLegLength, 3) - (VerticalLegLength - HorizontalLegThickness) * Math.Pow(HorizontalLegLength - VerticalLegThickness, 3)) -
                Area * Math.Pow(HorizontalLegLength - Centroid.X, 2);
        }

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

        protected override double CalculateJw()
        {
            return (Math.Pow(_horizontalLegLength - _verticalLegThickness / 2.0, 3.0) * Math.Pow(_horizontalLegThickness, 3.0) + Math.Pow(_verticalLegLength - _horizontalLegThickness / 2.0, 3.0) * Math.Pow(_verticalLegThickness, 3.0)) / 36.0; //CNR DT 208/2011
        }

        protected override double CalculateJt()
        {
            return 1.0 / 3.0 * (_horizontalLegLength - _verticalLegThickness / 2.0) * Math.Pow(_horizontalLegThickness, 3.0) + 1.0 / 3.0 * (_verticalLegLength - _horizontalLegThickness / 2.0) * Math.Pow(_verticalLegThickness, 3.0);
        }

        protected override Point2d CalculateShearCenter()
        {
            return new Point2d(_verticalLegThickness / 2.0, _horizontalLegThickness / 2.0);
        }

        protected override Point2d CalculateCentroid()
        {
            double xc = (_thinWalls[0].CalculateSy() + _thinWalls[1].CalculateSy()) / Area;
            double yc = (_thinWalls[0].CalculateSx() + _thinWalls[1].CalculateSx()) / Area;
            return new Point2d(xc, yc);
        }

        private void CalculateSection()
        {
            ThinWall thinWall1 = new ThinWall(HorizontalLegLength, HorizontalLegThickness, 0,
                new Point2d(HorizontalLegLength / 2, HorizontalLegThickness / 2));
            ThinWall thinWall2 = new ThinWall(VerticalLegLength - HorizontalLegThickness, VerticalLegThickness, Math.PI / 2,
                new Point2d(VerticalLegThickness / 2, HorizontalLegThickness + (VerticalLegLength - HorizontalLegThickness) / 2));

            SetThinWalls(new ThinWall[] { thinWall1, thinWall2 });

            SetMechanicalProperties();
            _shape = null;
            _mesh = GetMesh();
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return $"L {_verticalLegLength}x{_verticalLegThickness}x{_horizontalLegLength}x{_thinWalls}";
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionLVersion", version);

            info.AddValue("HorizontalLegLength", _horizontalLegLength);
            info.AddValue("HorizontalLegThickness", _horizontalLegThickness);
            info.AddValue("VerticalLegLength", _verticalLegLength);
            info.AddValue("VerticalLegThickness", _verticalLegThickness);
            info.AddValue("R", _r);
        }

        #endregion

        #region Equals, hashcode, operators

        public override bool Equals(object obj)
        {
            return Equals(obj as SectionL);
        }

        public bool Equals(SectionL other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   _horizontalLegLength == other._horizontalLegLength &&
                   _horizontalLegThickness == other._horizontalLegThickness &&
                   _verticalLegLength == other._verticalLegLength &&
                   _verticalLegThickness == other._verticalLegThickness &&
                   _r == other._r;
        }

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
                return hashCode;
            }
        }

        public static bool operator ==(SectionL left, SectionL right)
        {
            return EqualityComparer<SectionL>.Default.Equals(left, right);
        }

        public static bool operator !=(SectionL left, SectionL right)
        {
            return !(left == right);
        }

        #endregion
    }
}
