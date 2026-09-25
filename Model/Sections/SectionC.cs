using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    [Serializable]
    public class SectionC : ThinWallSection, ISerializable, IEquatable<SectionC>
    {
        #region Variables

        protected double _h;
        protected double _tw;
        protected double _lengthBottom;
        protected double _tBottom;
        protected double _lengthTop;
        protected double _tTop;

        private readonly double _r1;
        private readonly double _r2;

        #endregion

        #region Properties

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

        public override double Width => Math.Max(_lengthBottom, _lengthTop);

        public double HeightWeb => _h - _tBottom - _tTop;

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

        protected override double CalculateJw()
        {
            //CNR DT 208/2001
            double hf = _h - _tTop / 2.0 - _tBottom / 2.0;
            double length = _lengthBottom - _tw / 2.0;
            return hf * hf * Math.Pow(length, 3.0) * _tBottom / 12.0 * (2.0 * hf * _tw + 3.0 * length * _tBottom) / (hf * ThicknessWeb + 6.0 * length * _tBottom);
        }

        protected override double CalculateJt()
        {
            return 1.0 / 3.0 * (_lengthTop - _tw / 2.0) * Math.Pow(_tTop, 3.0) + 1.0 / 3.0 * (_h - _tTop / 2.0 - _tBottom / 2.0) *
                Math.Pow(_tw, 3.0) + 1.0 / 3.0 * (_lengthBottom - _tw / 2.0) * Math.Pow(_tBottom, 3.0);
        }

        protected override Point2d CalculateShearCenter()
        {
            //CNR DT 208/2001
            double hf = _h - _tTop / 2.0 - _tBottom / 2.0;
            double length = _lengthBottom - _tw / 2.0;
            double tf = _tBottom;
            return new Point2d(_tw / 2.0 - 3.0 * length * length * tf / (hf * _tw + 6.0 * length * tf), CalculateCentroid().Y);
        }

        protected override double CalculateWpl2()
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
                return base.CalculateWpl2();
        }

        protected override double CalculateWpl1()
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
                    throw new NotImplementedException("neutral axis in flange not yet supported");
            }
            else
                return base.CalculateWpl1();
        }

        protected override double CalculateWel2Min()
        {
            return J22 / DistanceXCentroidFromLeft();
        }

        protected override double CalculateWel2Max()
        {
            return J22 / DistanceXCentroidFromRight();
        }

        protected override double CalculateWel1Max()
        {
            return J11 / DistanceYCentroidFromTop();
        }

        protected override double CalculateWel1Min()
        {
            return J11 / DistanceYCentroidFromBottom();
        }

        protected override double CalculateWelXMax() => CalculateWel1Max();

        protected override double CalculateWelXMin() => CalculateWel1Min();

        protected override double CalculateWelYMax() => CalculateWel2Max();

        protected override double CalculateWelYMin() => CalculateWel2Min();

        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            if (_lengthTop == _lengthBottom && _tTop == _tBottom)
                return true;
            return false;
        }

        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return false;
        }

        public virtual double DistanceYCentroidFromBottom()
        {
            return CalculateCentroid().Y;
        }

        public virtual double DistanceYCentroidFromTop()
        {
            return Height - CalculateCentroid().Y;
        }

        public virtual double DistanceXCentroidFromRight()
        {
            return Math.Max(LengthTop, LengthBottom) - DistanceXCentroidFromLeft();
        }

        public virtual double DistanceXCentroidFromLeft()
        {
            return CalculateCentroid().X;
        }

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
            SetMechanicalProperties();
            _shape = null;
        }

        protected override double CalculateArea()
        {
            return base.CalculateArea() + CalculateAdditionalArea();
        }

        protected double CalculateAdditionalArea()
        {
            if (_edgeWorking == EdgeType.Chamfer)
                return 2 * Math.Pow((1.41 * R1), 2) / 2.0 -
                    2 * (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0);

            else if (_edgeWorking == EdgeType.Fillet)
                return 2 * (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) -
                    2 * (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0);

            else
                return 0.0;
        }

        protected override double CalculateJxx()
        {
            return base.CalculateJxx() + CalculateAdditionaJxx();
        }

        protected override double CalculateJyy()
        {
            return base.CalculateJyy() + CalculateAdditionaJyy();
        }

        private double CalculateAdditionaJxx()
        {
            if (_edgeWorking == EdgeType.Chamfer)
            {
                return 2.0 * (Math.Pow((1.41 * _r1), 4) / 24.0) +
                    Math.Pow((1.41 * R1), 2) / 2.0 * Math.Pow(Height - Centroid.Y - ThicknessTop - R1 / 3.5, 2) +
                    Math.Pow((1.41 * R1), 2) / 2.0 * Math.Pow(Centroid.Y - ThicknessBottom - R1 / 3.5, 2);
            }
            else if (_edgeWorking == EdgeType.Fillet)
            {
                return 2.0 * ((1.0 / 3.0) * Math.Pow(_r1, 4.0) - (Math.PI / 16.0) * Math.Pow(_r1, 4.0)) +
                    (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) * Math.Pow(Height - Centroid.Y - ThicknessTop - R1 / 3.5, 2) +
                    (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) * Math.Pow(Centroid.Y - ThicknessBottom - R1 / 3.5, 2);
            }
            else
                return 0.0;
        }

        private double CalculateAdditionaJyy()
        {
            if (_edgeWorking == EdgeType.Chamfer)
            {
                return 2.0 * (Math.Pow((1.41 * R1), 4) / 24.0 +
                    Math.Pow((1.41 * R1), 2) / 2.0 * Math.Pow(Centroid.X - ThicknessWeb - R1 / 3.5, 2));
            }
            else if (_edgeWorking == EdgeType.Fillet)
            {
                return 2.0 * ((1.0 / 3.0) * Math.Pow(R1, 4.0) - (Math.PI / 16.0) * Math.Pow(R1, 4.0) +
                    Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.Pow(Centroid.X - ThicknessWeb - R1 / 3.5, 2));
            }
            else
                return 0.0;
        }

        protected override Point2d CalculateCentroid()
        {
            double xSum = 0;
            double ySum = 0;
            double area = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                xSum += _thinWalls[i].CalculateSy();
                ySum += _thinWalls[i].CalculateSx();
                area += _thinWalls[i].Area;
            }

            if (R1 != 0)
            {
                if (_edgeWorking == EdgeType.Chamfer)
                {
                    xSum += 2 * Math.Pow((1.41 * R1), 2) / 2.0 *
                        Math.Abs(ThicknessWeb + R1 / 3.5);

                    ySum += Math.Pow((1.41 * R1), 2) / 2.0 *
                        Math.Abs(ThicknessBottom + R1 / 3.5);
                    ySum += Math.Pow((1.41 * R1), 2) / 2.0 *
                        Math.Abs(Height - ThicknessTop - R1 / 3.5);

                    area += 2 * Math.Pow((1.41 * R1), 2) / 2.0;
                }

                else if (_edgeWorking == EdgeType.Fillet)
                {
                    xSum += 2 * (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) *
                        Math.Abs(ThicknessWeb + R1 / 3.5);

                    ySum += (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) *
                        Math.Abs(ThicknessBottom + R1 / 3.5);
                    ySum += (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) *
                        Math.Abs(Height - ThicknessTop - R1 / 3.5);

                    area += 2 * (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0);

                }
            }

            if (R2 != 0)
            {
                xSum -= (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0) *
                    Math.Abs(LengthBottom - R2 / 3.5);
                xSum -= (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0) *
                    Math.Abs(LengthTop - R2 / 3.5);

                ySum -= (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0) *
                    Math.Abs(ThicknessBottom + R2 / 3.5);
                ySum -= (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0) *
                    Math.Abs(Height - ThicknessTop - R2 / 3.5);

                area -= 2 * (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0);
            }

            return new Point2d((xSum / area), (ySum / area));
        }

        #endregion

        #region Equals, hashcode, operators

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

        public override string ToString()
        {
            return $"C {_h}x{_tw}x{_lengthBottom}x{_tBottom}x{_lengthTop}x{_tTop} ";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SectionC);
        }

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

        public static bool operator ==(SectionC left, SectionC right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SectionC left, SectionC right)
        {
            return !(left == right);
        }

        #endregion

    }
}
