using System;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionC : ThinWallSection
    {
        #region Variables

        protected double _h;
        protected double _tw;
        protected double _lengthBottom;
        protected double _tBottom;
        protected double _lengthTop;
        protected double _tTop;

        #endregion


        #region Properties

        public double H => _h;

        public double Hw => _h - _tBottom - _tTop;

        public double Tw => _tw;

        public double LBottom => _lengthBottom;

        public double ThicknessBottom => _tBottom;

        public double LTop => _lengthTop;

        public double ThicknessTop => _tTop;

        #endregion


        #region Public Constructors

        public SectionC(double h, double tw, double lTop, double tTop, double lBottom, double tBottom, Material material, string name) 
            : base(material, name)
        {
            _h = h < 0 ? throw new ArgumentException($"height cannot be lower than zero") : h; 
            _lengthTop = lTop < 0 ? throw new ArgumentException($"Top lenght cannot be lower than zero") : lTop; 
            _lengthBottom = lBottom < 0 ? throw new ArgumentException($"Bottom lenght cannot be lower than zero") : lBottom; 
            _tBottom = tBottom < 0 ? throw new ArgumentException($"Bottom thickness cannot be lower than zero") : tBottom; 
            _tTop = tTop < 0 ? throw new ArgumentException($"Top thickness cannot be lower than zero") : tTop; 
            _tw = tw < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : tw;

            if (_lengthBottom == _lengthTop && _tTop == _tBottom)
                _isSymmetricAlongXLocalAxis = true;
            _isSymmetricAlongYLocalAxis = false;

            ThinWall web = new ThinWall(h, tw, Math.PI / 2, new Point2d(tw / 2, h / 2));
            ThinWall flangeTop = new ThinWall(LTop - Tw, ThicknessTop, 0, new Point2d(Tw + (LTop - Tw) / 2, ThicknessBottom + Hw + ThicknessTop / 2));
            ThinWall flangeBottom = new ThinWall(LBottom - Tw, ThicknessBottom, 0, new Point2d(Tw + (LBottom - Tw) / 2, ThicknessBottom / 2));

            ThinWalls = new ThinWall[] { web, flangeBottom, flangeTop };
        }

        #endregion


        #region Public override method

        public override double CalculateWel2()
        {
            return Math.Min(CalculateWelyyLeft(), CalculateWelyyRight());
        }

        public override double CalculateWel1()
        {
            return Math.Min(CalculateWelxxBottom(), CalculateWelxxTop());
        }

        public override double CalculateJw()
        {
            //CNR DT 208/2001
            double hf = _h - _tTop / 2.0 - _tBottom / 2.0;
            double length = _lengthBottom - _tw / 2.0;
            return hf * hf * Math.Pow(length, 3.0) * _tBottom / 12.0 * (2.0 * hf * _tw + 3.0 * length * _tBottom) / (hf * Tw + 6.0 * length * _tBottom);
        }

        public override double CalculateJt()
        {
            return 1.0 / 3.0 * (_lengthTop - _tw / 2.0) * Math.Pow(_tTop, 3.0) + 1.0 / 3.0 * (_h - _tTop / 2.0 - _tBottom / 2.0) * 
                Math.Pow(_tw, 3.0) + 1.0 / 3.0 * (_lengthBottom - _tw / 2.0) * Math.Pow(_tBottom, 3.0);
        }

        public override Point2d CalculateShearCenter()
        {
            //CNR DT 208/2001
            double hf = _h - _tTop / 2.0 - _tBottom / 2.0;
            double length = _lengthBottom - _tw / 2.0;
            double tf = _tBottom;
            return new Point2d(_tw / 2.0 - 3.0 * length * length * tf / (hf * _tw + 6.0 * length * tf), CalculateCentroid().Y);
        }

        public override string ToString()
        {
            string s = "C section: \n";
            s = s + "h = " + _h + " mm \n";
            s = s + "tw = " + _tw + " mm \n";
            s = s + "Length Bottom = " + _lengthBottom + " mm \n";
            s = s + "Thickness Bottom = " + _tBottom + " mm \n";
            s = s + "Length Top = " + _lengthTop + " mm \n";
            s = s + "Thickness Top = " + _tTop + " mm \n";
            return s;
        }

        public double CalculateWelyyLeft()
        {
            return J22 / DistanceXCentroidFromLeft();
        }

        public double CalculateWelyyRight()
        {
            return J22 / DistanceXCentroidFromRight();
        }

        public double CalculateWelxxTop()
        {
            return J11 / DistanceYCentroidFromTop();
        }

        public double CalculateWelxxBottom()
        {
            return J11 / DistanceYCentroidFromBottom();
        }

        public double DistanceYCentroidFromBottom()
        {
            return CalculateCentroid().Y;
        }

        public double DistanceYCentroidFromTop()
        {
            return H - CalculateCentroid().Y;
        }

        public double DistanceXCentroidFromRight()
        {
            return Math.Max(LTop, LBottom) - DistanceXCentroidFromLeft();
        }
        public double DistanceXCentroidFromLeft()
        {
            return CalculateCentroid().X;
        }


        public override double CalculateWpl2()
        {
            if (IsSymmetricAlongXLocalAxis)
            {
                if (_area / 2.0 > _h * _tw)
                {
                    double hDown = Area / 2.0 / (_tTop + _tBottom);
                    SectionT secTop = new SectionT(_lengthBottom - hDown, _h, _tBottom + _tTop, _tw, _material, string.Empty);
                    return Area / 2.0 * (hDown / 2.0 + secTop.DistanceYCentroidFromBottom());
                }
                else
                {
                    double tEff = Area / 2.0 / H;        // rettangolo alto H e spesso tEff
                    SectionC sectionC = new SectionC(H, Tw - tEff, LTop, ThicknessTop, LBottom, ThicknessBottom, _material, string.Empty);
                    return Area / 2.0 * (tEff / 2 + sectionC.DistanceXCentroidFromLeft());
                }
            }
            else
                throw new NotImplementedException("Different lenght or thickness not yet supported");
        }

        public override double CalculateWpl1()
        {
            if (IsSymmetricAlongXLocalAxis)
            {
                if (_area / 2.0 > _tTop * _lengthTop)
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

        #endregion
    }
}
