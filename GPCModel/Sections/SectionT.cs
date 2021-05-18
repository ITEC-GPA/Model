using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionT : ThinWallSection
    {
        #region Variables

        protected double _hw;
        protected double _tw;
        protected double _tf;
        protected double _b;

        #endregion


        #region Properties
        public double Hw => _hw;
        public double H => _hw + _tf;
        public double Tw => _tw;
        public double Tf => _tf;
        public double B => _b;

        public bool IsSymmetricAlongZLocalAxis = true;

        public bool IsSymmetricAlongYLocalAxis = false;

        #endregion


        #region Public Constructors

        public SectionT(double hw, double b, double tw, double tf, Material material, string name) 
            : base(material, name)
        {
            #region Check inputs

            _hw = hw < 0 ? throw new ArgumentException($"Web lenght cannot be lower than zero") : hw;                   // spessore anima;
            _b = b < 0 ? throw new ArgumentException($"Flange lenght cannot be lower than zero") : b;                   // spessore anima;
            _tw = tw < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : tw;                // spessore anima;
            _tf = tf < 0 ? throw new ArgumentException($"Flange thickness cannot be lower than zero") : tw;             // spessore flangia;

            #endregion

            ThinWall web = new ThinWall(hw, tw, Math.PI / 2, new Point2d(0, 0));
            ThinWall flange = new ThinWall(b, tf, 0, new Point2d(0, hw / 2 + tf / 2));

            ThinWalls = new ThinWall[] { web, flange };         
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
