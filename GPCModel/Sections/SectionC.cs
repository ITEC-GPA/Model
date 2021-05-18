using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionC : ThinWallSection
    {
        #region Variables

        protected double _h;
        protected double _hw;
        protected double _tw;
        protected double _lengthBottom;
        protected double _tBottom;
        protected double _lengthTop;
        protected double _tTop;

        internal ThinWall[] _wall = new ThinWall[3];

        #endregion


        #region Properties


        public double H => _h;

        public double Hw => _hw;

        public double Tw => _tw;

        public double LBottom => _lengthBottom;

        public double ThicknessBottom => _tBottom;

        public double LTop => _lengthTop;

        public double ThicknessTop => _tTop;

        public bool IsSymmetricAlongZLocalAxis => false;

        public bool IsSymmetricAlongYLocalAxis
        {
            get
            {
                if (_lengthBottom == _lengthTop && _tTop == _tBottom)
                    return true;
                else
                    return false;
            }
        }

        #endregion


        #region Public Constructors

        public SectionC(double h, double tw, double lTop, double tTop, double lBottom, double tBottom, Material material, string name) 
            : base(material, name)
        {
            _hw = h - tBottom - tTop;
            _h = h < 0 ? throw new ArgumentException($"height cannot be lower than zero") : h; 
            _lengthTop = lTop < 0 ? throw new ArgumentException($"Top lenght cannot be lower than zero") : lTop; 
            _lengthBottom = lBottom < 0 ? throw new ArgumentException($"Bottom lenght cannot be lower than zero") : lBottom; 
            _tBottom = tBottom < 0 ? throw new ArgumentException($"Bottom thickness cannot be lower than zero") : tBottom; 
            _tTop = tTop < 0 ? throw new ArgumentException($"Top thickness cannot be lower than zero") : tTop; 
            _tw = tw < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : tw; 

            double fyk = ((SteelMaterial)material).Fyk;

            ThinWall web = new ThinWall(h, tw, Math.PI / 2, new Point2d(0, 0));
            ThinWall flangeTop = new ThinWall(LTop, ThicknessTop, 0, new Point2d(0, Hw / 2 + ThicknessTop / 2));
            ThinWall flangeBottom = new ThinWall(LBottom, ThicknessBottom, 0, new Point2d(0, -Hw / 2 - ThicknessBottom / 2));

            ThinWalls = new ThinWall[] { web, flangeBottom, flangeTop };
        }

        #endregion


        #region Public override method

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
            return new Point2d(_tw / 2.0 - 3.0 * length * length * tf / (hf * _tw + 6.0 * length * tf), _centroid.Y);
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

        #endregion
    }
}
