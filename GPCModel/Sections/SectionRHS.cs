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

        double _h;
        double _b;
        double _tfTop;
        double _tfBottom;
        double _tw1;
        double _tw2;

        #endregion


        #region Properties

        public double B => _b;

        public double Bint => _b - _tw1 - _tw2;

        public double H => _h;

        public double Hw => _h - _tfBottom - _tfTop;

        public double TTop => _tfTop;

        public double TBottom => _tfBottom;

        public double TWebLeft => _tw1;

        public double TWebRight => _tw2;

        public bool IsSymmetricAlongYLocalAxis => _tw1 == _tw2;

        public bool IsSymmetricAlongXLocalAxis => _tfBottom == _tfTop;

        #endregion



        public SectionRHS(double h, double b, double tf_top, double tf_bottom, double tw1, double tw2, Material material, string name) 
            : base(material, name)
        {
            _angleX1 = 0;
            _h = h;
            _b = b;
            _tfTop = tf_top;
            _tfBottom = tf_bottom;
            _tw1 = tw1;
            _tw2 = tw2;

            ThinWall webSx = new ThinWall(Hw, _tw1, Math.PI / 2, new Point2d(-_b / 2 + tw1 / 2, 0));
            ThinWall webDx = new ThinWall(Hw, _tw2, Math.PI / 2, new Point2d(_b / 2 - tw2 / 2, 0));
            ThinWall flangeTop = new ThinWall(_b, _tfTop, 0, new Point2d(Hw / 2 + _tfTop / 2, 0));
            ThinWall flangeBottom = new ThinWall(_b, _tfBottom, 0, new Point2d(-Hw / 2 - _tfBottom / 2, 0));

            ThinWalls = new ThinWall[] { webSx, webDx, flangeBottom, flangeTop };
        }


        public double CalculateWel22Bottom()
        {
            return _jyy / (_h - _centroid.Y);
        }

        public double CalculateWel22Top()
        {
            return _jyy / Math.Abs(_centroid.Y);
        }

        public double CalculateWel11Left()
        {
            return _jxx / (_centroid.X);
        }

        public double CalculateWel11Right()
        {
            return _jxx / Math.Abs(_centroid.X - _b);
        }

        public double CalculateWpl11()
        {
            double ALeftface = (_tw1*Hw) + _tfTop * _tw1 + _tfBottom * _tw1;
            if (_area / 2.0 > ALeftface)
            {
                if (IsSymmetricAlongXLocalAxis)
                {
                    double hTSection = (_area / 2.0 - ALeftface) / (_tfTop + _tfBottom);
                    SectionT halfSectionLeft = new SectionT(hTSection + _tw1, _h, _tfTop + _tfBottom, _tw1, _material, string.Empty);
                    SectionT halfSectionRigth = new SectionT(_b - hTSection - _tw1, _h, _tfTop + _tfBottom, _tw2, _material, string.Empty);
                    return (_area / 2.0) * (halfSectionLeft.Centroid.Y + halfSectionRigth.Centroid.Y);
                }
                else
                    throw new Exception("different thickness not yet supported");
            }
            else            
                throw new Exception("not yet supported");
            
        }

        public double CalculateWpl22()
        {
            if (_area / 2.0 > (_tw2 * Hw)) //plateTop
            {
                if (IsSymmetricAlongYLocalAxis)
                {
                    double hTSection = (_area / 2.0 - (_tw2 * Hw)) / (_tw1 + _tw2);
                    SectionT halfSectionTop = new SectionT(hTSection + _tfTop, _b, _tw1 + _tw2, _tfTop, _material, string.Empty);
                    SectionT halfSectionBottom = new SectionT(_h - hTSection - _tfTop, _b, _tw1 + _tw2, _tfBottom, _material, string.Empty);
                    return (_area / 2.0) * (halfSectionTop.Centroid.Y + halfSectionBottom.Centroid.Y);
                }
                else
                    throw new Exception("different thickness not yet supported");
            }
            else            
                throw new Exception("not yet supported");            
        }

        #region Public override method

        public override Point2d CalculateShearCenter()
        {
            if (_tfBottom == _tfTop && _tw1 == _tw2)
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
            double Amed = (_h - (_tfTop / 2.0) - (_tfBottom / 2.0)) * (_b - (_tw1 / 2.0) - (_tw2 / 2.0));
            double LmedTop = _b - _tw1 / 2.0 - _tw2 / 2.0;
            double LmedBottom = LmedTop;
            double LmedWeb1 = _h - _tfTop / 2.0 - _tfBottom / 2.0;
            double LmedWeb2 = LmedWeb1;
            return  4.0 * Amed * Amed / (LmedBottom / _tfBottom + LmedTop / _tfTop + LmedWeb1 / _tw1 + LmedWeb2 / _tw2);
        }
        
        public override string ToString()
        {
            string s = "RHS section: \n";
            s = s + "Height = " + _h + " mm \n";
            s = s + "Thickness Web Left = " + _tw1 + " mm \n";
            s = s + "Thickness Web Rigth = " + _tw2 + " mm \n";
            s = s + "Length Bottom = " + _b + " mm \n";
            s = s + "Thickness Bottom = " + _tfBottom + " mm \n";
            s = s + "Length Top = " + _b + " mm \n";
            s = s + "Thickness Top = " + _tfTop + " mm \n";
            return s;
        }

        #endregion

        public double MinSigma(double N, double M2, double M1)
        {
            double sigma1 = N / _area - M2 / Jyy * (_h - _centroid.Y) + M1 / Jxx * (_centroid.X);
            double sigma2 = N / _area - M2 / Jyy * (_h - _centroid.Y) - M1 / Jxx * (_b - _centroid.X);
            double sigma3 = N / _area + M2 / Jyy * (_centroid.Y) + M1 / Jxx * (_centroid.X);
            double sigma4 = N / _area + M2 / Jyy * (_centroid.Y) - M1 / Jxx * (_b - _centroid.X);

            double sigmaMin = Math.Min(sigma1, sigma2);
            sigmaMin = Math.Min(sigmaMin, sigma3);
            sigmaMin = Math.Min(sigmaMin, sigma4);
            return sigmaMin;
        }


    }
}
