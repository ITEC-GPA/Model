using System;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionT : ThinWallSection
    {
        #region Variables

        protected double _h;
        protected double _tw;
        protected double _tf;
        protected double _b;

        #endregion


        #region Properties

        public double Height => _h;

        public double HeightWeb => _h - _tf;

        public double ThicknessWeb => _tw;

        public double ThicknessFlange => _tf;

        public double LenghtFlange => _b;

        #endregion


        #region Public Constructors

        public SectionT(double height, double flangeLength, double thicknessWeb, double thicknessFlange, Material material, string name) 
            : base(material, name)
        {
            #region Check inputs

            _h = height < 0 ? throw new ArgumentException($"Web lenght cannot be lower than zero") : height;                   // spessore anima;
            _b = flangeLength < 0 ? throw new ArgumentException($"Flange lenght cannot be lower than zero") : flangeLength;                   // spessore anima;
            _tw = thicknessWeb < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : thicknessWeb;                // spessore anima;
            _tf = thicknessFlange < 0 ? throw new ArgumentException($"Flange thickness cannot be lower than zero") : thicknessFlange;             // spessore flangia;

            #endregion

            _isSymmetricAlongYLocalAxis = true;
            _isSymmetricAlongXLocalAxis = false;

            ThinWall web = new ThinWall(HeightWeb, thicknessWeb, Math.PI / 2, new Point2d(LenghtFlange / 2, HeightWeb / 2));
            ThinWall flange = new ThinWall(flangeLength, thicknessFlange, 0, new Point2d(LenghtFlange / 2, HeightWeb + thicknessFlange / 2));

            ThinWalls = new ThinWall[] { web, flange };         
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

        public override double CalculateWpl1()
        {
            if (_area / 2.0 >= _b * _tf)
            {
                double yPlastic = _area / 2.0 / _tw;
                SectionT halfSectionTop = new SectionT(Height - yPlastic, _b, _tw, _tf, _material, string.Empty);
                return _area / 2.0 * (halfSectionTop.DistanceYCentroidFromBottom() + yPlastic / 2.0);
            }
            else
            {
                double hTopPlastic = (_area / 2.0) / _b;
                //can't use SectionT because infinite loop
                double Aweb = _tw * (Height - _tf);
                double Aflange = _b * (_tf - hTopPlastic);
                double S = Aweb * ((Height - _tf) / 2.0 + hTopPlastic) + Aflange * hTopPlastic / 2.0;
                return (_area / 2.0) * (hTopPlastic / 2.0 + S / (Aweb + Aflange));
            }
        }

        public override double CalculateWpl2()
        {
            return 1.0 / 4.0 * _tf * Math.Pow(_b, 2.0) + 1.0 / 4.0 * (Height - _tf) * Math.Pow(_tw, 2.0);
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
            return Height - CalculateCentroid().Y;
        }

        public double DistanceXCentroidFromRight()
        {
            return LenghtFlange - CalculateCentroid().X;
        }

        public double DistanceXCentroidFromLeft()
        {
            return CalculateCentroid().X;
        }

        #endregion


        #region Public override method

        public override Point2d CalculateShearCenter()
        {
            return new Point2d(_b / 2.0, Height - _tf / 2.0);
        }

        public override double CalculateJw()
        {
            return Math.Pow(_b, 3.0) * Math.Pow(_tf, 3.0) / 144.0 + Math.Pow(Height - _tf / 2.0, 3.0) * Math.Pow(_tw, 3.0) / 36.0; //Bleich 1952, Picard and Beaulieu 1991
        }

        public override double CalculateJt()
        {            
            return (_b * Math.Pow(_tf, 3.0) + (Height - _tf / 2.0) * Math.Pow(_tw, 3.0)) / 3.0;
        }

        public override string ToString()
        {
            string s = "T section: \n";
            s = s + "Height = " + Height + " mm \n";
            s = s + "Thickness Web = " + _tw + " mm \n";
            s = s + "Length Top = " + _b + " mm \n";
            s = s + "Thickness Top = " + _tf + " mm \n";
            return s;
        }

        #endregion

    }
}
