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

        public double Base => _b;

        public double BaseInternal => _b - _twL - _twR;

        public double Height => _h;

        public double Heightinternal => _h - _tfBottom - _tfTop;

        public double ThicknessTop => _tfTop;

        public double ThicknessBottom => _tfBottom;

        public double ThicknessWebLeft => _twL;

        public double ThicknessWebRight => _twR;

        #endregion


        #region Public Constructors

        public SectionRHS(double height, double width, double thicknessTopFlange, double thicknessBottomFlange, 
                            double thicknessWebLeft, double thickenssWebRight, Material material, string name) 
            : base(material, name)
        {
            _angleX1 = 0;
            _h = height;
            _b = width;
            _tfTop = thicknessTopFlange;
            _tfBottom = thicknessBottomFlange;
            _twL = thicknessWebLeft;
            _twR = thickenssWebRight;

            if (_tfBottom == _tfTop)
                _isSymmetricAlongXLocalAxis = true;
            if (_twL == _twR)
                _isSymmetricAlongYLocalAxis = true;

            ThinWall webSx = new ThinWall(Heightinternal, _twL, Math.PI / 2, new Point2d(_twL / 2, Heightinternal / 2 + _tfBottom));
            ThinWall webDx = new ThinWall(Heightinternal, _twR, Math.PI / 2, new Point2d(Base - _twR / 2, Heightinternal / 2 + _tfBottom));
            ThinWall flangeTop = new ThinWall(Base, _tfTop, 0, new Point2d(Base / 2, _tfBottom + Heightinternal + _tfTop / 2));
            ThinWall flangeBottom = new ThinWall(Base, _tfBottom, 0, new Point2d(Base / 2, _tfBottom / 2));

            ThinWalls = new ThinWall[] { webSx, webDx, flangeBottom, flangeTop };
        }

        #endregion


        #region Public method

        public override double CalculateWel2()
        {
            return Math.Min(CalculateWelyLeft(), CalculateWelyRight());
        }

        public override double CalculateWel1()
        {
            return Math.Min(CalculateWelxBottom(), CalculateWelxTop());
        }

        public double CalculateWelyLeft()
        {
            return J22 / DistanceXCentroidFromRight();
        }

        public double CalculateWelyRight()
        {
            return J22 / (_b - DistanceXCentroidFromRight());
        }

        public double CalculateWelxBottom()
        {
            return J11 / DistanceYCentroidFromBottom();
        }

        public double CalculateWelxTop()
        {
            return J11 / (Height - DistanceYCentroidFromBottom());
        }

        public double DistanceYCentroidFromBottom()
        {
            return CalculateCentroid().Y;
        }

        public double DistanceYCentroidFromTop()
        {
            return Height + CalculateCentroid().Y;
        }

        public double DistanceXCentroidFromRight()
        {
            return CalculateCentroid().X;
        }

        public double DistanceXCentroidFromLeft()
        {
            return Base - CalculateCentroid().X;
        }

        public override double CalculateWpl2()
        {
            if (_area / 2.0 >= _twL * Heightinternal +_tfTop * _twL + _tfBottom * _twL)
            {
                if (IsSymmetricAlongYLocalAxis)
                {
                    SectionC halfSectionLeft = new SectionC(Height, ThicknessWebLeft, Base/2, ThicknessTop, Base/2, ThicknessBottom, _material, string.Empty);
                    SectionC halfSectionRigth = new SectionC(Height, ThicknessWebRight, Base / 2, ThicknessTop, Base / 2, ThicknessBottom, _material, string.Empty);
                    return (_area / 2.0) * (halfSectionLeft.DistanceXCentroidFromRight() + halfSectionRigth.DistanceXCentroidFromRight());
                }
                else
                    throw new Exception("different thickness not yet supported");
            }
            else            
                throw new Exception("not yet supported");
            
        }

        public override double CalculateWpl1()
        {
            if (_area / 2.0 >= (_twR * Heightinternal)) //plateTop
            {
                if (IsSymmetricAlongXLocalAxis)
                {
                    SectionC halfSectionTop = new SectionC(Base, ThicknessTop, Height / 2, _twR, Height / 2, _twL, _material, string.Empty);
                    SectionC halfSectionBottom = new SectionC(Base, ThicknessBottom, Height / 2, _twL, Height / 2, _twR, Material, string.Empty);
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
