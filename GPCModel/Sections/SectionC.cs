using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionC : Section
    {
        #region Variables
        protected double _h;
        protected double _hw;
        protected double _tw;
        protected double _lengthBottom;
        protected double _tBottom;
        protected double _lengthTop;
        protected double _tTop;

        Plate[] _plates = new Plate[3];
        #endregion

        #region Properties
        public double H => _h;
        public double Hw => _hw;
        public double Tw => _tw;
        public double LBottom => _lengthBottom;
        public double ThicknessBottom => _tBottom;
        public double LTop => _lengthTop;
        public double ThicknessTop => _tTop;
        public Plate[] Plates => _plates;
        #endregion

        public SectionC(double h, double tw, double LTop, double tTop, double LBottom, double tBottom, Material material) : base(material)
        {
            _hw = h - tBottom - tTop;
            _h = h;
            _lengthTop = LTop;
            _lengthBottom = LBottom;
            _tBottom = tBottom;
            _tTop = tTop;
            _tw = tw;

            double fyk = ((SteelMaterial)material).Fyk;

            _plates[0] = new Plate(_tTop, 0.0, _h - _tTop /2.0, _lengthTop, _h - _tTop / 2.0, fyk, Plate.TypePlate.outer);
            _plates[1] = new Plate(_tw, _tw/2.0, _tBottom, _tw/2.0, _h - _tTop, fyk, Plate.TypePlate.inner);
            _plates[2] = new Plate(_tBottom, 0.0, _tBottom / 2.0, _lengthBottom, _tBottom / 2.0, fyk, Plate.TypePlate.outer);

            _area = 0;
            double Sx = 0;
            double Sy = 0;
            for (int i = 0; i < 3; i++)
            {
                _area = _area + _plates[i].Area;
                Sx = Sx + _plates[i].Area * _plates[i].Centroid.X;
                Sy = Sy + _plates[i].Area * _plates[i].Centroid.Y;
            }
            _centroid = new Geometry.Point2d(Sx / _area, Sy / _area);

            _j11 = 0;
            _j22 = 0;
            for (int i = 0; i < _plates.Count(); i++)
            {
                double areaPlate = _plates[i].Area;
                double yGPlate = _plates[i].Centroid.Y;
                double xGPlate = _plates[i].Centroid.X;
                double j11Plate = _plates[i].JzCentroid;
                double j22Plate = _plates[i].JyCentroid;

                _j11 = _j11 + j11Plate + areaPlate * Math.Pow(xGPlate - _centroid.X, 2.0);
                _j22 = _j22 + j22Plate + areaPlate * Math.Pow(yGPlate - _centroid.Y, 2.0);
            }

            _jt = 1.0 / 3.0 * (_lengthTop - _tw / 2.0) * Math.Pow(_tTop, 3.0) + 1.0 / 3.0 * (_h - _tTop/2.0 - _tBottom/2.0) * Math.Pow(_tw, 3.0) + 1.0 / 3.0 * (_lengthBottom - _tw / 2.0) * Math.Pow(_tBottom, 3.0);

            IsSymmetricAlongZLocalAxis = false;
            if (_lengthBottom == _lengthTop && _tTop == _tBottom)
            {
                IsSymmetricAlongYLocalAxis = true;

                //CNR DT 208/2001
                double hf = _h - _tTop / 2.0 - _tBottom / 2.0;
                double length = _lengthBottom - _tw / 2.0;
                double tf = _tBottom;
                _jw = hf * hf * Math.Pow(length, 3.0) * tf / 12.0 * (2.0 * hf * _tw + 3.0 * length * tf) / (hf*tw+6.0*length*tf);
                _shearCenter = new Geometry.Point2d(_tw/2.0-3.0*length*length*tf/(hf*_tw+6.0*length*tf),_centroid.Y);

                _wel11Left = _j11 / _centroid.X;
                _wel11Right = _j11 / Math.Max(_lengthBottom - _centroid.X, _lengthTop - _centroid.X);
                _wel22Bottom = _j22 / _centroid.Y;
                _wel22Top = _j22 / (_h - _centroid.Y);
 
                _wpl11 = 0;
                {
                    if (_area/2.0 > _h * _tw)
                    {
                        double hDown = _area / 2.0 / (_tTop + _tBottom);
                        SectionT secTop = new SectionT(_lengthBottom - hDown, _h, _tBottom + _tTop, _tw, material);
                        _wpl11 = _area / 2.0 * (hDown/2.0 + secTop.Centroid.Y);
                    } else
                    {
                        throw new Exception("neutral axis in web not yet supported");
                    }
                }

                _wpl22 = 0;
                {
                    if (_area/2.0 > _tTop * _lengthTop)
                    {
                        double hTop = _tTop + (_area / 2.0 - _tTop * _lengthTop) / _tw;
                        SectionT secTop = new SectionT(hTop, _lengthTop, _tw, _tTop, material);
                        SectionT secBottom = new SectionT(_h - hTop, _lengthBottom, _tw, _tBottom, material);
                        _wpl22 = _area / 2.0 * (secTop.Centroid.Y + secBottom.Centroid.Y);
                    }
                    else
                    {
                        throw new Exception("neutral axis in flange not yet supported");
                    }
                }
            } else
            {
                throw new Exception("Different lenght or thickness not yet supported");
            }
        }

        public override double MinSigma(double N, double M2, double M1)
        {
            if (_lengthBottom == _lengthTop && _tBottom == _tTop) {
                double sigmaP1 = N / _area - M2 / _j22 * (_h - _centroid.Y) + M1 / _j11 * (_centroid.X);
                double sigmaP2 = N / _area - M2 / _j22 * (_h - _centroid.Y) - M1 / _j11 * (_lengthTop - _centroid.X);
                double sigmaP3 = N / _area + M2 / _j22 * (_centroid.Y) + M1 / _j11 * (_centroid.X);
                double sigmaP4 = N / _area + M2 / _j22 * (_centroid.Y) - M1 / _j11 * (_lengthBottom - _centroid.X);

                double sigmaMin = Math.Min(sigmaP1, sigmaP2);
                sigmaMin = Math.Min(sigmaMin, sigmaP3);
                sigmaMin = Math.Min(sigmaMin, sigmaP4);

                return sigmaMin;
            } else
            {
                throw new Exception("calculation of unequal C not yet supported");
            }
        }
    }
}
