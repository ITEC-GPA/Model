using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionH : ThinWallSection
    {
        #region Variables

        protected readonly double _h;
        protected readonly double _tw;
        protected readonly double _ttop;
        protected readonly double _tbottom;
        protected readonly double _btop;
        protected readonly double _bbottom;

        #endregion


        #region Properties

        public double Height => _h;

        public double LenghtBottomFlange => _bbottom;

        public double LenghtTopFlange => _btop;

        public double ThicknessTopFlange => _ttop;

        public double ThicknessBottomFlange => _tbottom;

        public double ThicknessWeb => _tw;

        public double HeightWeb => Height - ThicknessBottomFlange - ThicknessTopFlange;

        #endregion


        #region Public Constructors

        public SectionH(double height, double thicknessWeb, double topFlangeLength, double topFlangeThickness, double bottomFlangeLength,
                        double bottomFlangeThickness, Material material, string name)
            : base(material, name)
        {
            #region Check inputs

            _h = height < 0 ? throw new ArgumentException($"Web lenght cannot be lower than zero") : height;                               // altezza anima
            _tw = thicknessWeb < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : thicknessWeb;                            // spessore anima
            _btop = topFlangeLength < 0 ? throw new ArgumentException($"Top flange lenght cannot be lower than zero") : topFlangeLength;                  // larghezza piattabanda superiore
            _bbottom = bottomFlangeLength < 0 ? throw new ArgumentException($"Bottom flange lenght cannot be lower than zero") : bottomFlangeLength;      // larghezza piattabanda inferiore
            _ttop = topFlangeThickness < 0 ? throw new ArgumentException($"Top flange thickness cannot be lower than zero") : topFlangeThickness;               // spessore piattabanda superiore
            _tbottom = bottomFlangeThickness < 0 ? throw new ArgumentException($"Bottom flange thickness cannot be lower than zero") : bottomFlangeThickness;   // spessore piattabanda inferiore

            #endregion

            if (_btop == _bbottom && _tbottom == _ttop)
                _isSymmetricAlongXLocalAxis = true;
            _isSymmetricAlongYLocalAxis = true;

            ThinWall web = new ThinWall(HeightWeb, thicknessWeb, Math.PI / 2);
            ThinWall flangeTop = new ThinWall(topFlangeLength, topFlangeThickness, 0);
            ThinWall flangeBottom = new ThinWall(bottomFlangeLength, bottomFlangeThickness, 0);

            Points = new Point2d[3] { new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2.0, ThicknessBottomFlange + HeightWeb / 2.0),
                                    new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2.0, bottomFlangeThickness + HeightWeb + topFlangeThickness / 2.0),
                                    new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2.0, bottomFlangeThickness / 2.0)};

            ThinWalls = new ThinWall[3] { web, flangeTop, flangeBottom };
        }

        #endregion


        public virtual double DistanceYCentroidFromBottom()
        {
            return CalculateCentroid().Y;
        }

        public virtual double DistanceYCentroidFromTop()
        {
            return Height - DistanceYCentroidFromBottom();
        }

        public virtual double DistanceXCentroidFromRight()
        {
            return CalculateCentroid().X;
        }

        protected override double CalculateWpl2()
        {
            SectionT halfSectionTop = new SectionT(LenghtTopFlange / 2.0, Height / 2.0, ThicknessTopFlange,
                ThicknessWeb / 2.0, Material, string.Empty);
            SectionT halfSectionBottom = new SectionT(LenghtBottomFlange / 2.0, Height / 2.0, ThicknessBottomFlange,
                ThicknessWeb / 2.0, Material, string.Empty);

            double d = (halfSectionTop.Area * (LenghtTopFlange / 2.0 - halfSectionTop.DistanceYCentroidFromBottom()) +
                halfSectionBottom.Area * (LenghtBottomFlange / 2.0 - halfSectionBottom.DistanceYCentroidFromBottom())) /
                (halfSectionBottom.Area + halfSectionTop.Area);

            return 2.0 * d * _area / 2.0;
        }

        protected override double CalculateWpl1()
        {
            if (_area / 2.0 >= LenghtTopFlange * ThicknessTopFlange && _area / 2.0 >= LenghtBottomFlange * ThicknessBottomFlange)
            {
                double hw = (_area / 2.0 - LenghtTopFlange * ThicknessTopFlange) / ThicknessWeb;

                SectionT halfSectionTop = new SectionT(hw + ThicknessTopFlange, LenghtTopFlange, ThicknessWeb,
                    ThicknessTopFlange, Material, string.Empty);
                SectionT halfSectionBottom = new SectionT(Height - ThicknessTopFlange - hw, LenghtBottomFlange,
                    ThicknessWeb, ThicknessBottomFlange, Material, string.Empty);

                return _area / 2.0 * (halfSectionTop.DistanceYCentroidFromBottom() + halfSectionBottom.DistanceYCentroidFromBottom());
            }
            else if (_area / 2.0 <= LenghtTopFlange * ThicknessTopFlange)
            {
                double hHalf = _area / 2.0 / LenghtTopFlange;

                SectionH halfSectionBottom = new SectionH(Height - hHalf, ThicknessWeb, LenghtTopFlange,
                    ThicknessTopFlange - hHalf, LenghtBottomFlange, ThicknessBottomFlange, Material, string.Empty);

                return _area / 2.0 * (hHalf / 2.0 + (Height - hHalf - halfSectionBottom.DistanceYCentroidFromBottom()));
            }
            else if (_area / 2.0 <= LenghtBottomFlange * ThicknessBottomFlange)
            {
                double hHalf = _area / 2.0 / LenghtBottomFlange;

                SectionH halfSectionBottom = new SectionH(Height - hHalf, ThicknessWeb, LenghtTopFlange, ThicknessTopFlange,
                    LenghtBottomFlange, ThicknessBottomFlange - hHalf, Material, string.Empty);

                return _area / 2.0 * (hHalf / 2.0 + halfSectionBottom.DistanceYCentroidFromBottom());
            }
            else
                throw new NotImplementedException("Cannot calculate Wpl : Plastic neutral axis in flanges...to be implemented");
        }

        protected override double CalculateWel2()
        {
            return Math.Min(CalculateWelyBottom(), CalculateWelyTop());
        }

        protected override double CalculateWel1()
        {
            return Math.Min(CalculateWelxBottom(), CalculateWelxTop());
        }

        public virtual double CalculateWelyBottom()
        {
            return J22 / (LenghtBottomFlange - DistanceXCentroidFromRight());
        }

        public virtual double CalculateWelyTop()
        {
            return J22 / (LenghtTopFlange - DistanceXCentroidFromRight());
        }

        public virtual double CalculateWelxBottom()
        {
            return J11 / DistanceYCentroidFromBottom();
        }

        public virtual double CalculateWelxTop()
        {
            return J11 / DistanceYCentroidFromTop();
        }


        #region Public override method

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

        protected override double CalculateJw()
        {
            double dmed = _h - ThicknessBottomFlange / 2.0 - ThicknessTopFlange / 2.0;
            double JFlTop = 1.0 / 12.0 * ThicknessTopFlange * Math.Pow(LenghtTopFlange, 3.0);
            double JFlBottom = 1.0 / 12.0 * ThicknessBottomFlange * Math.Pow(LenghtBottomFlange, 3.0);
            double jz = JFlTop + JFlBottom + 1.0 / 12.0 * HeightWeb * Math.Pow(ThicknessWeb, 3.0);

            // CNR DT208_2011
            return dmed * dmed * JFlBottom * JFlTop / jz;
        }

        public double CalculateJtSSRC1889()
        {
            double dmed = Height - ThicknessBottomFlange / 2.0 - _ttop / 2.0;
            return (LenghtTopFlange * Math.Pow(ThicknessTopFlange, 3.0) +
                LenghtBottomFlange * Math.Pow(ThicknessBottomFlange, 3.0) + dmed * Math.Pow(ThicknessWeb, 3.0)) / 3.0;
            //SSRC 1998 dice che Jt corretto si calcola come 1/3 * l * t^3 ma l'anima va considerata maggiorata di metà delle due flange (non va corretto con il fattore alpha)
        }

        public override string ToString()
        {
            string s = "H section: \n";
            s = s + "Height = " + Height.ToString() + " mm \n";
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
