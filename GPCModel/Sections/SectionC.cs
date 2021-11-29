using System;
using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionC : ThinWallSection, ISection
    {
        #region Variables

        protected readonly double _h;
        protected readonly double _tw;
        protected readonly double _lengthBottom;
        protected readonly double _tBottom;
        protected readonly double _lengthTop;
        protected readonly double _tTop;

        #endregion


        #region Properties

        public double Height => _h;

        public double HeightWeb => _h - _tBottom - _tTop;

        public double ThicknessWeb => _tw;

        public double LengthBottom => _lengthBottom;

        public double ThicknessBottom => _tBottom;

        public double LengthTop => _lengthTop;

        public double ThicknessTop => _tTop;

        #endregion


        #region Public Constructors

        public SectionC(double height, double thicknessWeb, double lengthTop, double thicknessTop, double lengthBottom, double thicknessBottom, Material material, string name)
            : base(material, name)
        {
            _h = height < 0 ? throw new ArgumentException($"height cannot be lower than zero") : height;
            _lengthTop = lengthTop < 0 ? throw new ArgumentException($"Top lenght cannot be lower than zero") : lengthTop;
            _lengthBottom = lengthBottom < 0 ? throw new ArgumentException($"Bottom lenght cannot be lower than zero") : lengthBottom;
            _tBottom = thicknessBottom < 0 ? throw new ArgumentException($"Bottom thickness cannot be lower than zero") : thicknessBottom;
            _tTop = thicknessTop < 0 ? throw new ArgumentException($"Top thickness cannot be lower than zero") : thicknessTop;
            _tw = thicknessWeb < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : thicknessWeb;

            ThinWall web = new ThinWall(height, thicknessWeb, Math.PI / 2.0);
            ThinWall flangeTop = new ThinWall(LengthTop - ThicknessWeb, ThicknessTop, 0);
            ThinWall flangeBottom = new ThinWall(LengthBottom - ThicknessWeb, ThicknessBottom, 0);

            SetThinWalls(new ThinWall[] { web, flangeBottom, flangeTop },
                new Point2d[] { new Point2d(thicknessWeb / 2.0, height / 2.0),
                new Point2d(ThicknessWeb + (LengthTop - ThicknessWeb) / 2.0, ThicknessBottom + HeightWeb + ThicknessTop / 2.0),
                new Point2d(ThicknessWeb + (LengthBottom - ThicknessWeb) / 2.0, ThicknessBottom / 2.0)});

            SetMechanicalProperties();
        }

        #endregion


        #region Public override method


        protected override Shape2d GetShape()
        {
            throw new NotImplementedException();
        }

        protected override double CalculateWel2()
        {
            return Math.Min(CalculateWelyyLeft(), CalculateWelyyRight());
        }

        protected override double CalculateWel1()
        {
            return Math.Min(CalculateWelxxBottom(), CalculateWelxxTop());
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
                    SectionT secTop = new SectionT(_lengthBottom - hDown, _h, _tBottom + _tTop, _tw, _material, string.Empty);
                    return Area / 2.0 * (hDown / 2.0 + secTop.DistanceYCentroidFromBottom());
                }
                else
                {
                    double tEff = Area / 2.0 / Height;        // rettangolo alto H e spesso tEff
                    SectionC sectionC = new SectionC(Height, ThicknessWeb - tEff, LengthTop, ThicknessTop, LengthBottom, ThicknessBottom, _material, string.Empty);
                    return Area / 2.0 * (tEff / 2 + sectionC.DistanceXCentroidFromLeft());
                }
            }
            else
                throw new NotImplementedException("Different lenght or thickness not yet supported");
        }

        protected override double CalculateWpl1()
        {
            if (IsSymmetricAlongXLocalAxis)
            {
                if (_area / 2.0 >= _tTop * _lengthTop)
                {
                    double hTop = _tTop + (_area / 2.0 - _tTop * _lengthTop) / _tw;
                    SectionT secTop = new SectionT(hTop, _lengthTop, _tw, _tTop, _material, string.Empty);
                    SectionT secBottom = new SectionT(_h - hTop, _lengthBottom, _tw, _tBottom, _material, string.Empty);
                    return _area / 2.0 * (secTop.DistanceYCentroidFromBottom() + secBottom.DistanceYCentroidFromBottom());
                }
                else
                    throw new NotImplementedException("neutral axis in flange not yet supported");
            }
            else
                throw new NotImplementedException("Different lenght or thickness not yet supported");
        }


        protected virtual double CalculateWelyyLeft()
        {
            return J22 / DistanceXCentroidFromLeft();
        }

        protected virtual double CalculateWelyyRight()
        {
            return J22 / DistanceXCentroidFromRight();
        }

        protected virtual double CalculateWelxxTop()
        {
            return J11 / DistanceYCentroidFromTop();
        }

        protected virtual double CalculateWelxxBottom()
        {
            return J11 / DistanceYCentroidFromBottom();
        }

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

        

        public override string ToString()
        {
            return $"C {_h}x{_tw}x{_lengthBottom}x{_tBottom}x{_lengthTop}x{_tTop} ";
        }


        #region Equals, hashcode, operators

        public override bool Equals(object obj)
        {
            return obj is SectionC c &&
                   base.Equals(obj) &&
                   _h == c._h &&
                   _tw == c._tw &&
                   _lengthBottom == c._lengthBottom &&
                   _tBottom == c._tBottom &&
                   _lengthTop == c._lengthTop &&
                   _tTop == c._tTop;
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
                return hashCode;
            }
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

        #endregion


    }
}
