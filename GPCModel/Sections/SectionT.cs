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

        public double H => _h;

        public double Hw => _h - _tf;

        public double Tw => _tw;

        public double Tf => _tf;

        public double B => _b;

        #endregion


        #region Public Constructors

        public SectionT(double h, double b, double tw, double tf, Material material, string name) 
            : base(material, name)
        {
            #region Check inputs

            _h = h < 0 ? throw new ArgumentException($"Web lenght cannot be lower than zero") : h;                   // spessore anima;
            _b = b < 0 ? throw new ArgumentException($"Flange lenght cannot be lower than zero") : b;                   // spessore anima;
            _tw = tw < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : tw;                // spessore anima;
            _tf = tf < 0 ? throw new ArgumentException($"Flange thickness cannot be lower than zero") : tf;             // spessore flangia;

            #endregion

            _isSymmetricAlongYLocalAxis = true;
            _isSymmetricAlongXLocalAxis = false;

            ThinWall web = new ThinWall(Hw, tw, Math.PI / 2, new Point2d(B / 2, Hw / 2));
            ThinWall flange = new ThinWall(b, tf, 0, new Point2d(B / 2, Hw + tf / 2));

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
            if (_area / 2.0 > _b * _tf)
            {
                double yPlastic = _area / 2.0 / _tw;
                SectionT halfSectionTop = new SectionT(H - yPlastic, _b, _tw, _tf, _material, string.Empty);
                return _area / 2.0 * (halfSectionTop.DistanceYCentroidFromBottom() + yPlastic / 2.0);
            }
            else
            {
                double hTopPlastic = (_area / 2.0) / _b;
                //can't use SectionT because infinite loop
                double Aweb = _tw * (H - _tf);
                double Aflange = _b * (_tf - hTopPlastic);
                double S = Aweb * ((H - _tf) / 2.0 + hTopPlastic) + Aflange * hTopPlastic / 2.0;
                return (_area / 2.0) * (hTopPlastic / 2.0 + S / (Aweb + Aflange));
            }
        }

        public override double CalculateWpl2()
        {
            return 1.0 / 4.0 * _tf * Math.Pow(_b, 2.0) + 1.0 / 4.0 * (H - _tf) * Math.Pow(_tw, 2.0);
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
            return J11 / (H - DistanceYCentroidFromBottom());
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
            return B - CalculateCentroid().X;
        }

        public double DistanceXCentroidFromLeft()
        {
            return CalculateCentroid().X;
        }

        #endregion


        #region Public override method

        public override Point2d CalculateShearCenter()
        {
            return new Point2d(_b / 2.0, H - _tf / 2.0);
        }

        public override double CalculateJw()
        {
            return Math.Pow(_b, 3.0) * Math.Pow(_tf, 3.0) / 144.0 + Math.Pow(H - _tf / 2.0, 3.0) * Math.Pow(_tw, 3.0) / 36.0; //Bleich 1952, Picard and Beaulieu 1991
        }

        public override double CalculateJt()
        {            
            return (_b * Math.Pow(_tf, 3.0) + (H - _tf / 2.0) * Math.Pow(_tw, 3.0)) / 3.0;
        }

        public override string ToString()
        {
            string s = "T section: \n";
            s = s + "Height = " + H + " mm \n";
            s = s + "Thickness Web = " + _tw + " mm \n";
            s = s + "Length Top = " + _b + " mm \n";
            s = s + "Thickness Top = " + _tf + " mm \n";
            return s;
        }

        #endregion

    }
}
