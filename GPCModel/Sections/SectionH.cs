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

        public double Height => _h;

        public double LenghtBottomFlange => _bbottom;

        public double LenghtTopFlange => _btop;

        public double ThicknessTopFlange => _ttop;

        public double ThicknessBottomFlange => _tbottom;

        public double ThicknessWeb => _tw;

        public double HeightWeb => _h - _ttop - _btop;

        public bool IsSymmetricAlongZLocalAxis => true;

        public bool IsSymmetricAlongYLocalAxis => _btop == _bbottom && _tbottom == _ttop;

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

            ThinWall web = new ThinWall(HeightWeb, tw, Math.PI / 2, new Point2d(0, 0));
            ThinWall flangeTop = new ThinWall(btop, ttop, 0, new Point2d(0, HeightWeb / 2 + ttop / 2));
            ThinWall flangeBottom = new ThinWall(bbottom, tbottom, 0, new Point2d(0, -HeightWeb / 2 - tbottom / 2));

            ThinWalls = new ThinWall[3] { web , flangeBottom, flangeTop };
        }

        #endregion


        #region Public override method

        public override Point2d CalculateShearCenter()
        {
            //CNR DT208_2011 --> to be checked
            double JFlTop = 1.0 / 12.0 * _ttop * Math.Pow(_btop, 3.0);
            double JFlBottom = 1.0 / 12.0 * _tbottom * Math.Pow(_bbottom, 3.0);
            double jz = JFlTop + JFlBottom + 1.0 / 12.0 * HeightWeb * Math.Pow(_tw, 3.0);
            double zBottom = _centroid.Y - _tbottom / 2.0;
            double zTop = _h - _ttop / 2.0 - _centroid.Y;

            return new Point2d(_centroid.X, _centroid.Y - (zBottom * JFlBottom - zTop * JFlTop) / jz);
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

        public override double CalculateJt()
        {
            double dmed = Height - _tbottom / 2.0 - _ttop / 2.0;
            return (_btop * Math.Pow(_ttop, 3.0) + _bbottom * Math.Pow(_tbottom, 3.0) + dmed * Math.Pow(_tw, 3.0)) / 3.0;   //SSRC 1998 -> Straus use this formula with _hw instead of dmed
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
