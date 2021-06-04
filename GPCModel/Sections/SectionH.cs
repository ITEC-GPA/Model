using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    public class SectionH : ThinWallSection
    {     
        #region Variables

        protected double _h;
        protected double _tw;
        protected double _ttop;
        protected double _tbottom;
        protected double _btop;
        protected double _bbottom;

        #endregion


        #region Properties

        public double H => _h;

        public double LenghtBottomFlange => _bbottom;

        public double LenghtTopFlange => _btop;

        public double ThicknessTopFlange => _ttop;

        public double ThicknessBottomFlange => _tbottom;

        public double ThicknessWeb => _tw;

        public double HeightWeb => H - ThicknessBottomFlange - ThicknessTopFlange;

        public bool IsISection => IsISectionCheck();

        public bool IsHSection => IsHSectionCheck();
        
        #endregion


        #region Public Constructors

        public SectionH(double h, double tw, double btop, double ttop, double bbottom, double tbottom, Material material, string name)
            : base(material, name)
        {
            #region Check inputs

            _h = h < 0 ? throw new ArgumentException($"Web lenght cannot be lower than zero") : h;                               // altezza anima
            _tw = tw < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : tw;                            // spessore anima
            _btop = btop < 0 ? throw new ArgumentException($"Top flange lenght cannot be lower than zero") : btop;                  // larghezza piattabanda superiore
            _bbottom = bbottom < 0 ? throw new ArgumentException($"Bottom flange lenght cannot be lower than zero") : bbottom;      // larghezza piattabanda inferiore
            _ttop = ttop < 0 ? throw new ArgumentException($"Top flange thickness cannot be lower than zero") : ttop;               // spessore piattabanda superiore
            _tbottom = tbottom < 0 ? throw new ArgumentException($"Bottom flange thickness cannot be lower than zero") : tbottom;   // spessore piattabanda inferiore

            #endregion

            if (_btop == _bbottom && _tbottom == _ttop)
                _isSymmetricAlongXLocalAxis = true;
            _isSymmetricAlongYLocalAxis = true;

            ThinWall web = new ThinWall(HeightWeb, tw, Math.PI / 2, new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2, ThicknessBottomFlange + HeightWeb / 2));
            ThinWall flangeTop = new ThinWall(btop, ttop, 0, new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2, tbottom + HeightWeb + ttop / 2));
            ThinWall flangeBottom = new ThinWall(bbottom, tbottom, 0, new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2, tbottom / 2));

            ThinWalls = new ThinWall[3] { web , flangeBottom, flangeTop };
        }

        #endregion

        public override double CalculateWel2()
        {
            return Math.Min(CalculateWelyBottom(), CalculateWelyTop());
        }

        public override double CalculateWel1()
        {
            return Math.Min(CalculateWelxBottom(), CalculateWelxTop());
        }

        public double CalculateWelyBottom()
        {
            return J22 / (LenghtBottomFlange - DistanceXCentroidFromRight());
        }

        public double CalculateWelyTop()
        {
            return J22 / (LenghtTopFlange - DistanceXCentroidFromRight());
        }

        public double CalculateWelxBottom()
        {
            return J11 / DistanceYCentroidFromBottom();
        }

        public double CalculateWelxTop()
        {
            return J11 / DistanceYCentroidFromTop();
        }

        private double DistanceYCentroidFromBottom()
        {
            return CalculateCentroid().Y;
        }

        public double DistanceYCentroidFromTop()
        {
            return H - DistanceYCentroidFromBottom();
        }

        private double DistanceXCentroidFromRight()
        {
            return CalculateCentroid().X;
        }

        public override double CalculateWpl2()
        {
            SectionT halfSectionTop = new SectionT(_btop / 2.0, H / 2.0, _ttop, _tw / 2.0, _material, string.Empty);
            SectionT halfSectionBottom = new SectionT(_bbottom / 2.0, H / 2.0, _tbottom, _tw / 2.0, _material, string.Empty);
            double d = (halfSectionTop.Area * (_btop / 2.0 - halfSectionTop.DistanceYCentroidFromBottom()) + halfSectionBottom.Area * (_bbottom / 2.0 - halfSectionBottom.DistanceYCentroidFromBottom())) / 
                (halfSectionBottom.Area + halfSectionTop.Area);
            return 2.0 * d * _area / 2.0;
        }

        public override double CalculateWpl1()
        {
            if (_area / 2.0 > _btop * _ttop && _area / 2.0 > _bbottom * _tbottom)
            {
                double hw = (_area / 2.0 - _btop * _ttop) / _tw;
                SectionT halfSectionTop = new SectionT(hw + _ttop, _btop, _tw, _ttop, _material, string.Empty);
                SectionT halfSectionBottom = new SectionT(H - _ttop - hw, _bbottom, _tw, _tbottom, _material, string.Empty);
                return _area / 2.0 * (halfSectionTop.DistanceYCentroidFromBottom() + halfSectionBottom.DistanceYCentroidFromBottom());
            }
            else if (_area / 2.0 <= _btop * _ttop)
            {
                double hHalf = _area / 2.0 / _btop;
                SectionH halfSectionBottom = new SectionH(H - hHalf, _tw, _btop, _ttop - hHalf, _bbottom, _tbottom, _material, string.Empty);
                return _area / 2.0 * (hHalf / 2.0 + (H - hHalf - halfSectionBottom.DistanceYCentroidFromBottom()));
            }
            else if (_area / 2.0 <= _bbottom * _tbottom)
            {
                double hHalf = _area / 2.0 / _bbottom;
                SectionH halfSectionBottom = new SectionH(H - hHalf, _tw, _btop, _ttop, _bbottom, _tbottom - hHalf, _material, string.Empty);
                return _area / 2.0 * (hHalf / 2.0 + halfSectionBottom.DistanceYCentroidFromBottom());
            }
            else            
                throw new NotImplementedException("Cannot calculate Wpl : Plastic neutral axis in flanges...to be implemented");            
        }

        private bool IsISectionCheck()
        {
            if (H / LenghtTopFlange < 1.5 && H / LenghtBottomFlange < 1.5)
                return true;
            return false;
        }

        private bool IsHSectionCheck()
        {
            if (IsISectionCheck())
                return false;
            return true;
        }


        #region Public override method

        public override Point2d CalculateShearCenter()
        {
            //CNR DT208_2011 --> to be checked
            double JFlTop = 1.0 / 12.0 * _ttop * Math.Pow(_btop, 3.0);
            double JFlBottom = 1.0 / 12.0 * _tbottom * Math.Pow(_bbottom, 3.0);
            double jz = JFlTop + JFlBottom + 1.0 / 12.0 * HeightWeb * Math.Pow(_tw, 3.0);
            double zBottom = CalculateCentroid().Y - _tbottom / 2.0;
            double zTop = _h - _ttop / 2.0 - CalculateCentroid().Y;

            return new Point2d(CalculateCentroid().X, CalculateCentroid().Y - (zBottom * JFlBottom - zTop * JFlTop) / jz);
        }

        public override double CalculateJw()
        {
            double dmed = _h - _tbottom / 2.0 - _ttop / 2.0;
            double JFlTop = 1.0 / 12.0 * _ttop * Math.Pow(_btop, 3.0);
            double JFlBottom = 1.0 / 12.0 * _tbottom * Math.Pow(_bbottom, 3.0);
            double jz = JFlTop + JFlBottom + 1.0 / 12.0 * HeightWeb * Math.Pow(_tw, 3.0);

            // CNR DT208_2011
            return dmed * dmed * JFlBottom * JFlTop / jz;
        }

        public double CalculateJtSSRC1889()
        {
            double dmed = H - _tbottom / 2.0 - _ttop / 2.0;
            return (_btop * Math.Pow(_ttop, 3.0) + _bbottom * Math.Pow(_tbottom, 3.0) + dmed * Math.Pow(_tw, 3.0)) / 3.0;
            //SSRC 1998 dice che Jt corretto si calcola come 1/3 * l * t^3 ma l'anima va considerata maggiorata di metà delle due flange (non va corretto con il fattore alpha)
        }

        public override string ToString()
        {
            string s = "H section: \n";
            s = s + "Height = " + H.ToString() + " mm \n";
            s = s + "Thickness Web = " + _tw + " mm \n";
            s = s + "Length Bottom = " + _bbottom + " mm \n";
            s = s + "Thickness Bottom = " + _tbottom + " mm \n";
            s = s + "Length Top = " + _btop + " mm \n";
            s = s + "Thickness Top = " + _ttop + " mm \n";
            return s;
        }

        #endregion
    }
}
