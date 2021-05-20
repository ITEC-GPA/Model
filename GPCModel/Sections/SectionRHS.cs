using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    public class SectionRHS : ThinWallSection
    {
        #region Varibles

        private readonly double _h;
        private readonly double _b;
        private readonly double _tfTop;
        private readonly double _tfBottom;
        private readonly double _twL;
        private readonly double _twR;

        #endregion


        #region Properties

        public double B => _b;

        public double Binternal => _b - _twL - _twR;

        public double H => _h;

        public double Hinternal => _h - _tfBottom - _tfTop;

        public double TTop => _tfTop;

        public double TBottom => _tfBottom;

        public double TWebLeft => _twL;

        public double TWebRight => _twR;

        #endregion


        #region Public Constructors

        public SectionRHS(double h, double b, double tf_top, double tf_bottom, double tw1, double tw2, Material material, string name) 
            : base(material, name)
        {
            _angleX1 = 0;
            _h = h;
            _b = b;
            _tfTop = tf_top;
            _tfBottom = tf_bottom;
            _twL = tw1;
            _twR = tw2;

            if (_tfBottom == _tfTop)
                _isSymmetricAlongXLocalAxis = true;
            if (_twL == _twR)
                _isSymmetricAlongYLocalAxis = true;

            ThinWall webSx = new ThinWall(Hinternal, _twL, Math.PI / 2, new Point2d(-_b / 2 + _twL / 2, 0));
            ThinWall webDx = new ThinWall(Hinternal, _twR, Math.PI / 2, new Point2d(_b / 2 - _twR / 2, 0));
            ThinWall flangeTop = new ThinWall(_b, _tfTop, 0, new Point2d(0, Hinternal / 2 + _tfTop / 2));
            ThinWall flangeBottom = new ThinWall(_b, _tfBottom, 0, new Point2d(0, -Hinternal / 2 - _tfBottom / 2));

            ThinWalls = new ThinWall[] { webSx, webDx, flangeBottom, flangeTop };
        }

        #endregion


        #region Public method

        public double CalculateWelyMin()
        {
            return Math.Min(CalculateWelyLeft(), CalculateWelyRight());
        }

        public double CalculateWelxMin()
        {
            return Math.Min(CalculateWelxBottom(), CalculateWelxTop());
        }

        public double CalculateWelyLeft()
        {
            return _jyy / DistanceXCentroidFromRight();
        }

        public double CalculateWelyRight()
        {
            return _jyy / (_b - DistanceXCentroidFromRight());
        }

        public double CalculateWelxBottom()
        {
            return _jxx / DistanceYCentroidFromBottom();
        }

        public double CalculateWelxTop()
        {
            return _jxx / (H - DistanceYCentroidFromBottom());
        }

        public double DistanceYCentroidFromBottom()
        {
            return H / 2 + CalculateCentroid().Y;
        }

        public double DistanceYCentroidFromTop()
        {
            return H / 2 + CalculateCentroid().Y;
        }

        public double DistanceXCentroidFromRight()
        {
            return B / 2 + CalculateCentroid().X;
        }

        public double DistanceXCentroidFromLeft()
        {
            return B / 2 - CalculateCentroid().X;
        }

        public double CalculateWplyy()
        {
            if (_area / 2.0 > _twL * Hinternal +_tfTop * _twL + _tfBottom * _twL)
            {
                if (IsSymmetricAlongXLocalAxis)
                {
                    SectionC halfSectionLeft = new SectionC(H, TWebRight, B/2, TTop, B/2, TBottom, _material, string.Empty);
                    SectionC halfSectionRigth = new SectionC(H, TWebLeft, B / 2, TTop, B / 2, TBottom, _material, string.Empty);
                    return (_area / 2.0) * (halfSectionLeft.DistanceXCentroidFromRight() + halfSectionRigth.DistanceXCentroidFromRight());
                }
                else
                    throw new Exception("different thickness not yet supported");
            }
            else            
                throw new Exception("not yet supported");
            
        }

        public double CalculateWplxx()
        {
            if (_area / 2.0 > (_twR * Hinternal)) //plateTop
            {
                if (IsSymmetricAlongYLocalAxis)
                {
                    SectionC halfSectionTop = new SectionC(B, TTop, H / 2, _twR, H / 2, _twL, _material, string.Empty);
                    SectionC halfSectionBottom = new SectionC(B, TBottom, H / 2, _twR, H / 2, _twL, Material, string.Empty);
                    return (_area / 2.0) * (halfSectionTop.DistanceXCentroidFromRight() + halfSectionBottom.DistanceXCentroidFromRight());
                }
                else
                    throw new Exception("different thickness not yet supported");
            }
            else            
                throw new Exception("not yet supported");            
        }

        #endregion


        #region Public override method

        public override Point2d CalculateShearCenter()
        {
            if (_tfBottom == _tfTop && _twL == _twR)
                return _centroid;
            else
                throw new Exception("Section RHS with different thickness not yet implemented");            
        }

        public override double CalculateJw()
        {
            return 0;
        }

        public override double CalculateJt()
        {
            double Amed = (_h - (_tfTop / 2.0) - (_tfBottom / 2.0)) * (_b - (_twL / 2.0) - (_twR / 2.0));
            double LmedTop = _b - _twL / 2.0 - _twR / 2.0;
            double LmedBottom = LmedTop;
            double LmedWeb1 = _h - _tfTop / 2.0 - _tfBottom / 2.0;
            double LmedWeb2 = LmedWeb1;
            return  4.0 * Amed * Amed / (LmedBottom / _tfBottom + LmedTop / _tfTop + LmedWeb1 / _twL + LmedWeb2 / _twR);
        }
        
        public override string ToString()
        {
            string s = "RHS section: \n";
            s = s + "Height = " + _h + " mm \n";
            s = s + "Thickness Web Left = " + _twL + " mm \n";
            s = s + "Thickness Web Rigth = " + _twR + " mm \n";
            s = s + "Length Bottom = " + _b + " mm \n";
            s = s + "Thickness Bottom = " + _tfBottom + " mm \n";
            s = s + "Length Top = " + _b + " mm \n";
            s = s + "Thickness Top = " + _tfTop + " mm \n";
            return s;
        }

        #endregion


    }
}
