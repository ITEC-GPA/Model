using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionT : Section
    {
        #region Variables
        protected double _h;
        protected double _tw;
        protected double _tf;
        protected double _b;

        protected Plate[] _plates;
        #endregion

        #region Properties
        public double H => _h;
        public double Tw => _tw;
        public double Tf => _tf;
        public double B => _b;
        public Plate[] Plates => _plates;
        #endregion

        public SectionT(double h, double b, double tw, double tf, Material material) : base(material)
        {
            _h = h;
            _b = b;
            _tw = tw;
            _tf = tf;

            double fy = ((SteelMaterial)material).Fyk;

            IsSymmetricAlongZLocalAxis = false;
            IsSymmetricAlongYLocalAxis = true;

            _plates = new Plate[3];

            _plates[0] = new Plate(_tf, _b / 2.0, _h - _tf / 2.0, 0, _h - _tf / 2.0, fy, Plate.TypePlate.outer);
            _plates[1] = new Plate(_tf, _b / 2.0, _h - _tf / 2.0, _b, _h - _tf / 2.0, fy, Plate.TypePlate.outer);
            _plates[2] = new Plate(_tw, _b / 2.0, _h - _tf, _b / 2.0, 0, fy, Plate.TypePlate.outer);

            _area = _plates[0].Area + _plates[1].Area + _plates[2].Area;

            double Sy = 0;
            for (int i = 0; i < _plates.Count(); i++)
            {
                Sy = Sy + _plates[i].Area * _plates[i].Centroid.Y;
            }

            _centroid = new Point2d(_b / 2, Sy / _area);

            _j22 = 0;
            _j11 = 0;
            for (int i = 0; i < _plates.Count(); i++)
            {
                _j22 = _j22 + _plates[i].JyCentroid + _plates[i].Area * Math.Pow(_plates[i].Centroid.Y - _centroid.Y, 2.0);
                _j11 = _j11 + _plates[i].JzCentroid + _plates[i].Area * Math.Pow(_plates[i].Centroid.X - _centroid.X, 2.0);
            }

            _wel22Top = _j22 / (_h - _centroid.Y);
            _wel22Bottom = _j22 / (_centroid.Y);
            _wel11Left = _j11 / (_centroid.X);
            _wel11Right = _j11 / (_b - _centroid.X);

            _wpl22 = 0;
            if (_area / 2.0 > _b * _tf)
            {
                double hHalftArea = _area / 2.0 / _tw;
                SectionT halfSectionTop = new SectionT(_h - hHalftArea, _b, _tw, _tf, material);
                SectionRectangular halfSectionBottom = new SectionRectangular(_tw, hHalftArea, material);
                _wpl22 = _area / 2.0 * (halfSectionTop.Centroid.Y + halfSectionBottom.Centroid.Y);
            } else
            {
                //throw new Exception("Neutral Axis in flange not yet implemented");
            }

            _wpl11 = 1.0 / 4.0 * _tf * Math.Pow(_b, 2.0) + 1.0 / 4.0 * (_h - _tf) * Math.Pow(_tw, 2.0);

            _jt = (_b * Math.Pow(_tf, 3.0) + (_h - _tf / 2.0) * Math.Pow(_tw, 3.0)) / 3.0;

            _jw = Math.Pow(_b, 3.0) * Math.Pow(_tf, 3.0) / 144.0 + Math.Pow(_h - _tf / 2.0, 3.0) * Math.Pow(_tw, 3.0) / 36.0; //Bleich 1952, Picard and Beaulieu 1991

            _shearCenter = new Point2d(_b / 2.0, _h - _tf /2.0);
        }

        public override double MinSigma(double N, double My, double Mz)
        {
            double sigmaP1 = N / _area - My / _wel22Top + Mz / _wel11Left;
            double sigmaP2 = N / _area - My / _wel22Top - Mz /_wel11Right;
            double sigmaP3 = N / _area + My / _wel22Bottom;
           
            double sigmaMin = Math.Min(sigmaP1, sigmaP2);
            sigmaMin = Math.Min(sigmaMin, sigmaP3);

            return sigmaMin;
        }
    }
}
