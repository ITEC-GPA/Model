using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    public class SectionRHS : Section
    {
        #region Varibles
        double _h;
        double _b;
        double _tf_top;
        double _tf_bottom;
        double _tw1;
        double _tw2;

        double _hw;

        bool _isHotFinished;

        protected Plate[] _plates;
        #endregion

        #region Properties
        public double B => _b;
        public double Bint => _b - _tw1 - _tw2;
        public double H => _h;
        public double Hw => _hw;
        public double ThicknessFlange {
            get {
                if(_tf_bottom == _tf_top) {
                    return _tf_top;
                } else
                {
                    throw new Exception("not yet supported");
                }
            }
        }
        public double TTop => _tf_top;
        public double TBottom => _tf_bottom;
        public double ThicknessWeb
        {
            get
            {
                if (_tw1 == _tw2)
                {
                    return _tw1;
                }
                else
                {
                    throw new Exception("not yet supported");
                }
            }
        }
        public double TWebLeft => _tw1;
        public double TWebRight => _tw2;

        public bool IsColdFormed {
            get => !_isHotFinished;
            set { _isHotFinished = !value; 
            }
        }
        public bool IsHotFinished {
            get => _isHotFinished;
            set { _isHotFinished = value;
            }
        }

        public Plate[] Plates { get => _plates; }
        #endregion

        public SectionRHS(double h, double b, double tf_top, double tf_bottom, double tw1, double tw2, bool isHotFinished, Materials.Material material) : base(material)
        {
            _material = material;
            _angleX1 = 0;

            _h = h;
            _b = b;
            _tf_top = tf_top;
            _tf_bottom = tf_bottom;
            _tw1 = tw1;
            _tw2 = tw2;

            _hw = h - tf_bottom - tf_top;

            _isHotFinished = isHotFinished;

            if (_tw1 == _tw2)
            {
                IsSymmetricAlongYLocalAxis = true;
            } else
            {
                IsSymmetricAlongYLocalAxis = false;
            }
            if (_tf_bottom == _tf_top)
            {
                IsSymmetricAlongZLocalAxis = true;
            } else
            {
                IsSymmetricAlongZLocalAxis = false;
            }

            _plates = new Plate[4];

            double fy = ((SteelMaterial)_material).Fyk;
            _plates[0] = new Plate(_tf_top, -_b / 2.0, _h / 2.0 - _tf_top / 2.0, _b / 2.0, _h / 2.0 - _tf_top / 2.0, fy, Plate.TypePlate.inner);
            _plates[1] = new Plate(_tf_bottom, -_b / 2.0, -_h / 2.0 + _tf_bottom / 2.0, _b / 2.0, -_h / 2.0 + _tf_bottom / 2.0, fy, Plate.TypePlate.inner);      
            _plates[2] = new Plate(_tw1, -_b / 2.0 + _tw1 / 2.0, -_h / 2.0 + _tf_bottom, -_b / 2.0 + _tw1 / 2.0, _h / 2.0 - _tf_top, fy, Plate.TypePlate.inner);
            _plates[3] = new Plate(_tw2, _b / 2.0 - _tw2 / 2.0, -_h / 2.0 + _tf_bottom, _b / 2.0 - _tw2 / 2.0, _h / 2.0 - _tf_top, fy, Plate.TypePlate.inner);

            _area = 0;
            double Sy = 0;
            double Sx = 0;
            for (int i = 0; i < _plates.Count(); i++)
            {
                Plate plate = _plates[i];
                Point2d centerPlate = _plates[i].Centroid;
                _area = _area + plate.Area;
                Sy = Sy + plate.Area * centerPlate.Y;
                Sx = Sx + plate.Area * centerPlate.X;
            }
            _centroid = new Point2d(Sx / _area, Sy / _area);
            _shearCenter = _centroid;

            _j22 = 0;
            _j11 = 0;
            for (int i = 0; i < _plates.Count(); i++)
            {
                Plate plate = _plates[i];
                Point2d centerPlate = _plates[i].Centroid;

                _j11 = _j11 + plate.JzCentroid + plate.Area * Math.Pow(centerPlate.X - _centroid.X, 2.0);
                _j22 = _j22 + plate.JyCentroid + plate.Area * Math.Pow(centerPlate.Y - _centroid.Y, 2.0);

            }

            _wel22Top = _j22 / (_h - _centroid.Y);
            _wel22Bottom = _j22 / (_centroid.Y);
            _wel11Left = _j11 / (_centroid.X);
            _wel11Right = _j11 / (_b - _centroid.X);

            _wpl22 = 0;
            if (_area/2.0 > _plates[0].Area) //plateTop
            {
                if (_tw1 == _tw2) {
                    double hTSection = (_area / 2.0 - _plates[0].Area) / (_tw1 + _tw2);
                    SectionT sec = new SectionT(hTSection + _tf_top, _b, _tw1 + _tw2, _tf_top, _material);
                    _wpl22 = _area / 2.0 * 2.0 * sec.Centroid.Y;
                } else
                {
                    throw new Exception("different thickness not yet supported");
                }
            }
            else
            {
                throw new Exception("not yet supported");
            }

            _wpl11 = 0;
            double ALeftface = _plates[2].Area + _tf_top * _tw1 + _tf_bottom * _tw1;
            if (_area / 2.0 > ALeftface)
            {
                if (_tf_bottom == _tf_top)
                {
                    double hTSection = (_area / 2.0 - ALeftface) / (_tf_top + _tf_bottom);
                    SectionT sec = new SectionT(hTSection + _tw1, _h, _tf_top + _tf_bottom, _tf_top, _material);
                    _wpl11 = _area / 2.0 * 2.0 * sec.Centroid.Y;
                }
                else
                {
                    throw new Exception("different thickness not yet supported");
                }
            }
            else
            {
                throw new Exception("not yet supported");
            }

            //Jt
            if (_tf_bottom == _tf_top && _tw1 == _tw2 && _tw1 == _tf_top)
            {
                double t = _tf_top;
                double rc = 0; // rc = 1.5 * t for RHS with corner
                double Ap = (_h - t) * (_b-t) - rc *rc * (4.0 - Math.PI);
                double p = 2.0 * ((_h - t) + (_b - t)) - 2.0 * rc * (4.0 - Math.PI);
                _jt = 4.0 * Ap * Ap * t / p; //Salmon & Johnson 1980
            } else
            {
                double Amed = (_h - (_tf_top / 2.0) - (_tf_bottom / 2.0)) * (_b - (_tw1 / 2.0) - (tw2 / 2.0));
                double LmedTop = _b - _tw1 / 2.0 - _tw2 / 2.0; 
                double LmedBottom = LmedTop;
                double LmedWeb1 = _h - _tf_top / 2.0 - _tf_bottom / 2.0;
                double LmedWeb2 = LmedWeb1;
                _jt = 4.0 * Amed / (_tf_top / LmedTop + _tf_bottom / LmedBottom + _tw1 / LmedWeb1 + _tw2 / LmedWeb2); /*to be checked*/
            }

            //Jw
            _jw = 0;
        }
    }
}
